using Bookings.Common.ValueObjects;
using Bookings.Contracts;

using BookingService.Application.Commands;
using BookingService.Application.Interfaces;
using BookingService.Domain.Aggregates.Booking;

using MassTransit;

using MediatR;

using Microsoft.Extensions.DependencyInjection;

using System;

namespace BookingService.Infrastructure.Messaging.MassTransit;

public class BookingStateMachine : MassTransitStateMachine<BookingState>
{
    public State AwaitingPayment { get; private set; } = default!;
    public State Reserved { get; private set; } = default!;
    public State Confirmed { get; private set; } = default!;
    public State Cancelled { get; private set; } = default!;
    public State Failed { get; private set; } = default!;

    public Schedule<BookingState, PaymentTimeoutExpired> PaymentTimeout { get; private set; } = default!;

    // Events
    public Event<CreateBooking> CreateBookingEvt { get; private set; } = default!;
    public Event<PaymentAuthorized> PaymentAuthorizedEvt { get; private set; } = default!;
    public Event<PaymentFailed> PaymentFailedEvt { get; private set; } = default!;
    public Event<PmsConfirmed> PmsConfirmedEvt { get; private set; } = default!;
    public Event<CancelBooking> CancelBookingEvt { get; private set; } = default!;

    public BookingStateMachine()
    {
        InstanceState(x => x.CurrentState);

        //Event(() => CreateBookingEvt, x => x.CorrelateById(m => m.Message.BookingId));
        Event(() => CreateBookingEvt, x =>
        {
            x.CorrelateById(m => m.Message.BookingId);
            x.InsertOnInitial = true;
            x.SetSagaFactory(ctx => new BookingState
            {
                CorrelationId = ctx.Message.BookingId,
                Created = DateTime.UtcNow
            });
        });
        Event(() => PaymentAuthorizedEvt, x => x.CorrelateById(m => m.Message.BookingId));
        Event(() => PaymentFailedEvt, x => x.CorrelateById(m => m.Message.BookingId));
        Event(() => PmsConfirmedEvt, x => x.CorrelateById(m => m.Message.BookingId));
        Event(() => CancelBookingEvt, x => x.CorrelateById(m => m.Message.BookingId));

        Schedule(() => PaymentTimeout, x => x.PaymentTimeoutTokenId, s =>
        {
            s.Delay = TimeSpan.FromMinutes(15);
            s.Received = e => e.CorrelateById(m => m.Message.BookingId);
        });

        Initially(
            When(CreateBookingEvt)
                .ThenAsync(async ctx =>
                {
                    var mediator = GetRequired<IMediator, CreateBooking>(ctx);
                    var repo = GetRequired<IBookingRepository, CreateBooking>(ctx);

                    var b = await repo.GetAsync(ctx.Saga.CorrelationId) 
                        ?? throw new InvalidOperationException($"Booking {ctx.Saga.CorrelationId} not found");

                    await mediator.Send(
                        new SetBookingStatusCommand(ctx.Saga.CorrelationId, BookingStatus.AwaitingPayment), ctx.CancellationToken);
                    await mediator.Send(
                        new ReserveInventoryCommand(ctx.Saga.CorrelationId), ctx.CancellationToken);
                    await mediator.Send(
                        new CreatePaymentCommand(ctx.Saga.CorrelationId), ctx.CancellationToken);
                    //await ctx
                    //    .GetPayload<ConsumeContext>()
                    //    .Publish(new BookingCreated(b.Id, b.TotalPrice), ctx.CancellationToken);
                })
                .Schedule(PaymentTimeout, ctx =>
                    new PaymentTimeoutExpired(ctx.Saga.CorrelationId))
                .TransitionTo(AwaitingPayment)
        );

        During(AwaitingPayment,
            When(PaymentAuthorizedEvt)
                .Then(ctx => {
                    ctx.Saga.PaymentRef = ctx.Message.PaymentProviderRef;
                    ctx.Saga.PaymentRefUpdated = DateTime.UtcNow;
                })
                .ThenAsync(async ctx =>
                {
                    var mediator = GetRequired<IMediator, PaymentAuthorized>(ctx);
                    await mediator.Send(new SetBookingStatusCommand(ctx.Saga.CorrelationId, BookingStatus.Reserved), ctx.CancellationToken);
                    await mediator.Send(new RequestPmsConfirmationCommand(ctx.Saga.CorrelationId), ctx.CancellationToken);
                })
                .Unschedule(PaymentTimeout)
                .TransitionTo(Reserved),

            When(PaymentFailedEvt)
                .ThenAsync(async ctx =>
                {
                    var mediator = GetRequired<IMediator, PaymentFailed>(ctx);
                    await mediator.Send(new RefundPaymentCommand(ctx.Saga.CorrelationId), ctx.CancellationToken);
                    await mediator.Send(new SetBookingStatusCommand(ctx.Saga.CorrelationId, BookingStatus.Failed), ctx.CancellationToken);
                })
                .Publish(ctx =>
                    new BookingCancelled(ctx.Saga.CorrelationId, ctx.Message.Error))
                .TransitionTo(Failed),

            When(PaymentTimeout.Received)
                .ThenAsync(async ctx => {
                    var mediator = GetRequired<IMediator>(ctx);
                    await mediator.Send(new SetBookingStatusCommand(ctx.Saga.CorrelationId, BookingStatus.Expired), ctx.CancellationToken);
                })
                .Publish(ctx => new CancelBooking(ctx.Saga.CorrelationId, "Payment timeout"))
                .TransitionTo(Cancelled)
        );

        During(Reserved,
            When(PmsConfirmedEvt)
                .Then(ctx => {
                    ctx.Saga.PmsNumber = ctx.Message.PmsNumber;
                    ctx.Saga.PmsNumberUpdated = DateTime.UtcNow;
                })
                .ThenAsync(async ctx => {
                    var mediator = GetRequired<IMediator, PmsConfirmed>(ctx);
                    await mediator.Send(new SetBookingStatusCommand(ctx.Saga.CorrelationId, BookingStatus.Confirmed), ctx.CancellationToken);
                })
                .TransitionTo(Confirmed),

            When(CancelBookingEvt)
                .ThenAsync(async ctx =>
                {
                    var mediator = GetRequired<IMediator, CancelBooking>(ctx);
                    await mediator.Send(new RefundPaymentCommand(ctx.Saga.CorrelationId), ctx.CancellationToken);
                    await mediator.Send(new SetBookingStatusCommand(ctx.Saga.CorrelationId, BookingStatus.Cancelled), ctx.CancellationToken);
                })
                .TransitionTo(Cancelled)
        );

        DuringAny(
            When(CancelBookingEvt)
                .ThenAsync(async ctx =>
                {
                    var mediator = GetRequired<IMediator, CancelBooking>(ctx);
                    await mediator.Send(new RefundPaymentCommand(ctx.Saga.CorrelationId), ctx.CancellationToken);
                    await mediator.Send(new SetBookingStatusCommand(ctx.Saga.CorrelationId, BookingStatus.Cancelled), ctx.CancellationToken);
                })
                .TransitionTo(Cancelled)
        );
    }

    // Overloads to support contexts with and without TData (Automatonymous BehaviorContext<TSaga> / BehaviorContext<TSaga,TData>)
    static T GetRequired<T>(BehaviorContext<BookingState> context) where T : notnull
    {
        var consume = context.GetPayload<ConsumeContext>();
        var provider = ResolveProvider(consume);
        return provider.GetRequiredService<T>();
    }

    static T GetRequired<T, TData>(BehaviorContext<BookingState, TData> context)
        where T : notnull
        where TData : class
    {
        var consume = context.GetPayload<ConsumeContext>();
        var provider = ResolveProvider(consume);
        return provider.GetRequiredService<T>();
    }

    static IServiceProvider ResolveProvider(ConsumeContext consume)
    {
        if (consume.TryGetPayload<IServiceProvider>(out var provider))
            return provider;
        if (consume.TryGetPayload<IServiceScope>(out var scope))
            return scope.ServiceProvider;
        throw new InvalidOperationException("IServiceProvider was not found in ConsumeContext payloads");
    }

    public record PaymentTimeoutExpired(Guid BookingId);
}