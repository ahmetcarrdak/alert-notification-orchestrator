# Alert Notification Service

Alertmanager webhook'larını alarak Microsoft Teams'e Adaptive Card formatında bildirim gönderen .NET 8 servisi.

## Mimari

Clean Architecture + CQRS (MediatR) yapısı kullanılmıştır.

```
src/
├── AlertNotificationService.Domain          # Entity, enum, arayüzler
├── AlertNotificationService.Application     # CQRS handler'ları, DTO'lar, davranışlar
├── AlertNotificationService.Infrastructure  # DB, HTTP istemcileri, harici servisler
└── AlertNotificationService.API             # Controller'lar, middleware
```

## Akış

```
Prometheus → Alertmanager → POST /webhook/alertmanager
                                      ↓
                              Alert PostgreSQL'e kaydedilir
                                      ↓
                          SEQ'ten uygulama logları çekilir
                                      ↓
                     Teams'e Adaptive Card gönderilir (Power Automate)
```

## Gereksinimler

- Docker & Docker Compose
- Harici `docker_default` ağında çalışan `seq` ve `alertmanager` container'ları

## Kurulum

```bash
docker compose up -d --build
```

API `http://localhost:8080` adresinde ayağa kalkar, migration otomatik uygulanır.

## Konfigürasyon

| Ortam Değişkeni | Açıklama |
|---|---|
| `ConnectionStrings__DefaultConnection` | PostgreSQL bağlantı dizesi |
| `Teams__WebhookUrl` | Power Automate webhook URL'i |
| `Seq__Url` | SEQ sunucu adresi (örn. `http://seq:5341`) |

## Endpoint'ler

| Method | Path | Açıklama |
|---|---|---|
| `POST` | `/webhook/alertmanager` | Alertmanager webhook alıcısı |
| `GET` | `/api/alerts` | Kayıtlı alert'leri listeler |

## Teams Bildirimi

Her alert için kart içeriği:

- **HTTP Status** / **Servis** / **Endpoint** / **Tarih** — FactSet
- **Hata Açıklaması** — Prometheus annotation'ından gelen özet
- **Hata Logu** *(tıklanabilir)* — SEQ'ten çekilen son 5 Error/Warning logu

## Docker Ağı

`alertmanager` ve `seq` container'larının bu servisle aynı ağda olması gerekir:

```bash
docker network connect alert-notification-service_default alertmanager
```

`docker-compose.yml` zaten `docker_default` harici ağına bağlanacak şekilde yapılandırılmıştır.
