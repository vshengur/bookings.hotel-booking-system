up:
	docker compose -f docker-compose.infra.yml up -d

down:
	docker compose -f docker-compose.infra.yml down

logs:
	docker compose -f docker-compose.infra.yml logs -f

restart:
	docker compose -f docker-compose.infra.yml restart

ps:
	docker compose -f docker-compose.infra.yml ps

clean:
	docker compose -f docker-compose.infra.yml down -v

dev:
	docker compose -f docker-compose.infra.yml -f docker-compose.override.yml up -d

k8s-apply:
	kubectl apply -f k8s/infra-k8s.yaml

k8s-delete:
	kubectl delete -f k8s/infra-k8s.yaml
