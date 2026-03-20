# ER Diagram — VPSI Parkovaci system

> Automaticky generovany diagram databazoveho schematu.

```mermaid
erDiagram
    PARKING_LOT ||--o{ PARKING_SPOT : "ma mista"
    PARKING_LOT ||--o{ DEVICE : "ma zarizeni"
    PARKING_LOT ||--o{ PARKING_SESSION : "obsahuje relace"
    PARKING_LOT ||--o{ TARIFF : "ma tarify"

    VEHICLE ||--o{ PARKING_SESSION : "parkuje"

    TARIFF ||--o{ TARIFF_RULE : "ma pravidla"
    TARIFF ||--o{ PARKING_SESSION : "uctuje"

    PARKING_SESSION ||--o{ PAYMENT_INTENT : "ma platby"

    DEVICE ||--o{ DEVICE_EVENT : "emituje eventy"

    APP_USER ||--o{ AUDIT_LOG : "provadi akce"

    PARKING_LOT {
        uuid id PK
        text name
        text address
        int capacity_total
        text timezone
        boolean is_active
        boolean is_deleted
        timestamptz created_at
        timestamptz updated_at
    }

    PARKING_SPOT {
        uuid id PK
        uuid parking_lot_id FK
        text code
        text spot_type "STANDARD|EV|ZTP|MOTORCYCLE"
        boolean is_active
        boolean is_deleted
        timestamptz created_at
        timestamptz updated_at
    }

    VEHICLE {
        uuid id PK
        text plate_number "GDPR"
        text country_code
        text plate_hash UK
        text vehicle_group
        boolean is_deleted
        timestamptz created_at
        timestamptz updated_at
    }

    DEVICE {
        uuid id PK
        uuid parking_lot_id FK
        text name
        text device_type "LPR|BARRIER|SENSOR|DISPLAY|TERMINAL"
        text protocol "REST|MQTT"
        jsonb config
        timestamptz last_seen_at
        boolean is_active
        boolean is_deleted
        timestamptz created_at
        timestamptz updated_at
    }

    TARIFF {
        uuid id PK
        uuid parking_lot_id FK
        text name
        text description
        boolean is_active
        timestamptz valid_from
        timestamptz valid_to
        boolean is_deleted
        timestamptz created_at
        timestamptz updated_at
    }

    TARIFF_RULE {
        uuid id PK
        uuid tariff_id FK
        text rule_type "PER_HOUR|FLAT|DAILY_CAP|FREE_MINUTES|NIGHT_RATE"
        numeric amount
        text currency
        jsonb parameters
        int priority
        timestamptz created_at
    }

    PARKING_SESSION {
        uuid id PK
        uuid parking_lot_id FK
        uuid vehicle_id FK
        uuid tariff_id FK
        timestamptz entry_at
        timestamptz exit_at
        text status "ACTIVE|PAID|CLOSED|DISPUTED|CANCELLED"
        uuid entry_device_id
        uuid exit_device_id
        numeric total_amount
        text currency
        timestamptz paid_at
        timestamptz created_at
        timestamptz updated_at
    }

    PAYMENT_INTENT {
        uuid id PK
        uuid parking_session_id FK
        numeric amount
        text currency
        text method "CARD|MOBILE_APP|QR"
        text provider_ref
        text status "INITIATED|AUTHORIZED|CAPTURED|FAILED|REFUNDED"
        timestamptz created_at
        timestamptz updated_at
    }

    DEVICE_EVENT {
        uuid id PK
        uuid device_id "logicky FK"
        text event_type
        timestamptz occurred_at PK
        text idempotency_key
        text plate_number
        jsonb payload
        text processing_status "RECEIVED|PROCESSING|PROCESSED|FAILED"
        timestamptz processed_at
        timestamptz created_at
    }

    APP_USER {
        uuid id PK
        text external_subject UK
        text display_name
        text email
        text role "ADMIN|TECHNICIAN|FINANCE"
        boolean is_active
        boolean is_deleted
        timestamptz created_at
        timestamptz updated_at
    }

    AUDIT_LOG {
        uuid id PK
        uuid app_user_id FK
        text action
        text entity_type
        uuid entity_id
        jsonb old_values
        jsonb new_values
        inet ip_address
        timestamptz created_at PK
    }
```

## Poznamky

- **device_event** a **audit_log** jsou partitioned tabulky (range by mesic) — composite PK `(id, occurred_at)` resp. `(id, created_at)`
- **device_event.device_id** nema FK constraint kvuli partitioning limitaci PostgreSQL
- **parking_session.entry_device_id/exit_device_id** jsou logicke odkazy bez FK constraint
- **vehicle.plate_number** je osobni udaj dle GDPR — reseno pres plate_hash pro vyhledavani
