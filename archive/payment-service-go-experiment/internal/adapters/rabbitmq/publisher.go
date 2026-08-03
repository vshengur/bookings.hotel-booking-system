package rabbit

import (
	"encoding/json"

	"payment-service/internal/logging"

	amqp "github.com/rabbitmq/amqp091-go"
)

type Publisher struct {
	ch   *amqp.Channel
	conn *amqp.Connection
	log  *logging.Logger
}

func NewPublisher(url string, log *logging.Logger) (*Publisher, error) {
	conn, err := amqp.Dial(url)
	if err != nil {
		return nil, err
	}
	ch, err := conn.Channel()
	if err != nil {
		conn.Close()
		return nil, err
	}
	return &Publisher{ch: ch, conn: conn, log: log}, nil
}

func (p *Publisher) Publish(exchange string, routingKey string, payload any) error {
	_ = p.ch.ExchangeDeclare(exchange, "topic", true, false, false, false, nil)
	b, _ := json.Marshal(payload)
	return p.ch.Publish(exchange, routingKey, false, false, amqp.Publishing{
		ContentType: "application/json",
		Body:        b,
	})
}

func (p *Publisher) Close() {
	_ = p.ch.Close()
	_ = p.conn.Close()
}
