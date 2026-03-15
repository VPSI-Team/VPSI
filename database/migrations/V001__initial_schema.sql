-- V001: Inicializace databázového schématu VPSI
-- Parkovací systém — kompletní DDL migrace
-- Cílová databáze: PostgreSQL 13+

-- =============================================================================
-- Nastavení prostředí
-- =============================================================================

SET search_path TO public;
SET timezone TO 'UTC';

-- Rozšíření pro kryptografické funkce (gen_random_uuid je vestavěné v PG 13+,
-- pgcrypto přidáváme pro budoucí šifrování citlivých dat)
CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- =============================================================================
-- 1. parking_lot — Parkoviště
-- =============================================================================

CREATE TABLE parking_lot (
    id              UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    name            TEXT        NOT NULL,
    address         TEXT,
    capacity_total  INT         NOT NULL CHECK (capacity_total >= 0),
    timezone        TEXT        NOT NULL DEFAULT 'Europe/Prague',
    is_active       BOOLEAN     NOT NULL DEFAULT true,
    is_deleted      BOOLEAN     NOT NULL DEFAULT false,
    deleted_at      TIMESTAMPTZ,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now()
);

COMMENT ON TABLE parking_lot IS 'Parkoviště — základní entita systému';
COMMENT ON COLUMN parking_lot.id IS 'Primární klíč parkoviště';
COMMENT ON COLUMN parking_lot.name IS 'Název parkoviště';
COMMENT ON COLUMN parking_lot.address IS 'Adresa parkoviště';
COMMENT ON COLUMN parking_lot.capacity_total IS 'Celková kapacita parkoviště (počet míst)';
COMMENT ON COLUMN parking_lot.timezone IS 'Časová zóna parkoviště pro zobrazení lokálního času';
COMMENT ON COLUMN parking_lot.is_active IS 'Příznak aktivního parkoviště';
COMMENT ON COLUMN parking_lot.is_deleted IS 'Příznak soft-delete';
COMMENT ON COLUMN parking_lot.deleted_at IS 'Čas soft-delete';

-- =============================================================================
-- 2. parking_spot — Parkovací místo
-- =============================================================================

CREATE TABLE parking_spot (
    id              UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    parking_lot_id  UUID        NOT NULL REFERENCES parking_lot(id),
    code            TEXT        NOT NULL,
    spot_type       TEXT        NOT NULL CHECK (spot_type IN ('STANDARD', 'EV', 'ZTP', 'MOTORCYCLE')),
    is_active       BOOLEAN     NOT NULL DEFAULT true,
    is_deleted      BOOLEAN     NOT NULL DEFAULT false,
    deleted_at      TIMESTAMPTZ,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    UNIQUE (parking_lot_id, code)
);

COMMENT ON TABLE parking_spot IS 'Parkovací místo — konkrétní stání v rámci parkoviště';
COMMENT ON COLUMN parking_spot.parking_lot_id IS 'Vazba na parkoviště';
COMMENT ON COLUMN parking_spot.code IS 'Kód místa (např. A-01, B-12)';
COMMENT ON COLUMN parking_spot.spot_type IS 'Typ místa: STANDARD, EV (elektro), ZTP (handicap), MOTORCYCLE';
COMMENT ON COLUMN parking_spot.is_deleted IS 'Příznak soft-delete';

-- =============================================================================
-- 3. vehicle — Vozidlo
-- =============================================================================

CREATE TABLE vehicle (
    id              UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    plate_number    TEXT        NOT NULL,
    country_code    TEXT        DEFAULT 'CZ',
    plate_hash      TEXT        NOT NULL UNIQUE,
    vehicle_group   TEXT,
    is_deleted      BOOLEAN     NOT NULL DEFAULT false,
    deleted_at      TIMESTAMPTZ,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now()
);

COMMENT ON TABLE vehicle IS 'Vozidlo — registrační značka (GDPR citlivý údaj)';
COMMENT ON COLUMN vehicle.plate_number IS 'Registrační značka vozidla — osobní údaj dle GDPR';
COMMENT ON COLUMN vehicle.country_code IS 'Kód země registrace (ISO 3166-1 alpha-2)';
COMMENT ON COLUMN vehicle.plate_hash IS 'Hash registrační značky pro rychlé vyhledávání bez odhalení původní hodnoty';
COMMENT ON COLUMN vehicle.vehicle_group IS 'Skupina vozidla: RESIDENT, EMPLOYEE, VIP nebo null';

-- =============================================================================
-- 4. device — Zařízení (LPR kamery, závory, senzory, displeje, terminály)
-- =============================================================================

CREATE TABLE device (
    id              UUID        PRIMARY KEY, -- ID se nastavuje explicitně z konfigurace
    parking_lot_id  UUID        NOT NULL REFERENCES parking_lot(id),
    name            TEXT        NOT NULL,
    device_type     TEXT        NOT NULL CHECK (device_type IN ('LPR', 'BARRIER', 'SENSOR', 'DISPLAY', 'TERMINAL')),
    protocol        TEXT        NOT NULL CHECK (protocol IN ('REST', 'MQTT')),
    config          JSONB       NOT NULL DEFAULT '{}',
    last_seen_at    TIMESTAMPTZ,
    is_active       BOOLEAN     NOT NULL DEFAULT true,
    is_deleted      BOOLEAN     NOT NULL DEFAULT false,
    deleted_at      TIMESTAMPTZ,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now()
);

COMMENT ON TABLE device IS 'Zařízení parkovacího systému — kamery, závory, senzory, displeje, terminály';
COMMENT ON COLUMN device.id IS 'ID zařízení — nastavuje se explicitně z konfigurace (NE auto-generated)';
COMMENT ON COLUMN device.device_type IS 'Typ zařízení: LPR (kamera), BARRIER (závora), SENSOR, DISPLAY, TERMINAL';
COMMENT ON COLUMN device.protocol IS 'Komunikační protokol: REST nebo MQTT';
COMMENT ON COLUMN device.config IS 'Konfigurace zařízení v JSON formátu';
COMMENT ON COLUMN device.last_seen_at IS 'Poslední komunikace se zařízením (heartbeat)';

-- =============================================================================
-- 5. tariff — Tarif
-- =============================================================================

CREATE TABLE tariff (
    id              UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    parking_lot_id  UUID        NOT NULL REFERENCES parking_lot(id),
    name            TEXT        NOT NULL,
    description     TEXT,
    is_active       BOOLEAN     NOT NULL DEFAULT true,
    valid_from      TIMESTAMPTZ NOT NULL,
    valid_to        TIMESTAMPTZ,
    is_deleted      BOOLEAN     NOT NULL DEFAULT false,
    deleted_at      TIMESTAMPTZ,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now()
);

COMMENT ON TABLE tariff IS 'Tarif — cenová politika parkoviště';
COMMENT ON COLUMN tariff.parking_lot_id IS 'Vazba na parkoviště';
COMMENT ON COLUMN tariff.valid_from IS 'Platnost tarifu od';
COMMENT ON COLUMN tariff.valid_to IS 'Platnost tarifu do (null = neomezeno)';

-- =============================================================================
-- 6. tariff_rule — Pravidlo tarifu
-- =============================================================================

CREATE TABLE tariff_rule (
    id              UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    tariff_id       UUID        NOT NULL REFERENCES tariff(id) ON DELETE CASCADE,
    rule_type       TEXT        NOT NULL CHECK (rule_type IN ('PER_HOUR', 'FLAT', 'DAILY_CAP', 'FREE_MINUTES', 'NIGHT_RATE')),
    amount          NUMERIC(12, 2) NOT NULL CHECK (amount >= 0),
    currency        TEXT        NOT NULL DEFAULT 'CZK',
    parameters      JSONB       NOT NULL DEFAULT '{}',
    priority        INT         NOT NULL DEFAULT 0,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now()
);

COMMENT ON TABLE tariff_rule IS 'Pravidlo tarifu — konkrétní cenové pravidlo v rámci tarifu';
COMMENT ON COLUMN tariff_rule.rule_type IS 'Typ pravidla: PER_HOUR, FLAT, DAILY_CAP, FREE_MINUTES, NIGHT_RATE';
COMMENT ON COLUMN tariff_rule.amount IS 'Částka pravidla (v měně currency)';
COMMENT ON COLUMN tariff_rule.parameters IS 'Parametry pravidla v JSON (např. {"from_hour": 22, "to_hour": 6} pro NIGHT_RATE)';
COMMENT ON COLUMN tariff_rule.priority IS 'Priorita pravidla — vyšší číslo = vyšší priorita';

-- =============================================================================
-- 7. parking_session — Parkovací relace
-- =============================================================================

CREATE TABLE parking_session (
    id              UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    parking_lot_id  UUID        NOT NULL REFERENCES parking_lot(id),
    vehicle_id      UUID        NOT NULL REFERENCES vehicle(id),
    tariff_id       UUID        REFERENCES tariff(id),
    entry_at        TIMESTAMPTZ NOT NULL,
    exit_at         TIMESTAMPTZ,
    status          TEXT        NOT NULL CHECK (status IN ('ACTIVE', 'PAID', 'CLOSED', 'DISPUTED', 'CANCELLED')),
    entry_device_id UUID,       -- Logický odkaz na device (BEZ FK — device_event je partitioned)
    exit_device_id  UUID,       -- Logický odkaz na device (BEZ FK — device_event je partitioned)
    total_amount    NUMERIC(12, 2),
    currency        TEXT        NOT NULL DEFAULT 'CZK',
    paid_at         TIMESTAMPTZ,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now()
);

COMMENT ON TABLE parking_session IS 'Parkovací relace — záznam o parkování vozidla od vjezdu po výjezd';
COMMENT ON COLUMN parking_session.vehicle_id IS 'Vazba na vozidlo';
COMMENT ON COLUMN parking_session.tariff_id IS 'Použitý tarif (null pokud zatím neurčen)';
COMMENT ON COLUMN parking_session.entry_at IS 'Čas vjezdu';
COMMENT ON COLUMN parking_session.exit_at IS 'Čas výjezdu (null = stále parkuje)';
COMMENT ON COLUMN parking_session.status IS 'Stav relace: ACTIVE, PAID, CLOSED, DISPUTED, CANCELLED';
COMMENT ON COLUMN parking_session.entry_device_id IS 'Zařízení vjezdu (logický odkaz, bez FK constraint)';
COMMENT ON COLUMN parking_session.exit_device_id IS 'Zařízení výjezdu (logický odkaz, bez FK constraint)';
COMMENT ON COLUMN parking_session.total_amount IS 'Celková částka za parkování';
COMMENT ON COLUMN parking_session.paid_at IS 'Čas zaplacení';

-- =============================================================================
-- 8. payment_intent — Platební záměr
-- =============================================================================

CREATE TABLE payment_intent (
    id                  UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    parking_session_id  UUID        NOT NULL REFERENCES parking_session(id),
    amount              NUMERIC(12, 2) NOT NULL CHECK (amount >= 0),
    currency            TEXT        NOT NULL DEFAULT 'CZK',
    method              TEXT        NOT NULL CHECK (method IN ('CARD', 'MOBILE_APP', 'QR')),
    provider_ref        TEXT,
    status              TEXT        NOT NULL CHECK (status IN ('INITIATED', 'AUTHORIZED', 'CAPTURED', 'FAILED', 'REFUNDED')),
    created_at          TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at          TIMESTAMPTZ NOT NULL DEFAULT now()
);

COMMENT ON TABLE payment_intent IS 'Platební záměr — záznam o pokusu o platbu za parkování';
COMMENT ON COLUMN payment_intent.parking_session_id IS 'Vazba na parkovací relaci';
COMMENT ON COLUMN payment_intent.amount IS 'Částka platby';
COMMENT ON COLUMN payment_intent.method IS 'Způsob platby: CARD, MOBILE_APP, QR';
COMMENT ON COLUMN payment_intent.provider_ref IS 'Reference platební brány (pro rekonciliaci)';
COMMENT ON COLUMN payment_intent.status IS 'Stav platby: INITIATED, AUTHORIZED, CAPTURED, FAILED, REFUNDED';

-- =============================================================================
-- 9. device_event — Události ze zařízení (partitioned by RANGE on occurred_at)
-- =============================================================================

CREATE TABLE device_event (
    id                  UUID        NOT NULL DEFAULT gen_random_uuid(),
    device_id           UUID        NOT NULL, -- Logický odkaz na device (BEZ FK — partitioned tabulka)
    event_type          TEXT        NOT NULL,
    occurred_at         TIMESTAMPTZ NOT NULL,
    idempotency_key     TEXT        NOT NULL,
    plate_number        TEXT,
    payload             JSONB       NOT NULL,
    processing_status   TEXT        NOT NULL DEFAULT 'RECEIVED'
                        CHECK (processing_status IN ('RECEIVED', 'PROCESSING', 'PROCESSED', 'FAILED')),
    processed_at        TIMESTAMPTZ,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT now(),
    PRIMARY KEY (id, occurred_at),
    UNIQUE (device_id, idempotency_key, occurred_at)
) PARTITION BY RANGE (occurred_at);

COMMENT ON TABLE device_event IS 'Události ze zařízení — partitioned podle occurred_at pro výkon';
COMMENT ON COLUMN device_event.device_id IS 'Logický odkaz na zařízení (bez FK kvůli partitioning)';
COMMENT ON COLUMN device_event.event_type IS 'Typ události (např. LPR_READ, BARRIER_OPEN)';
COMMENT ON COLUMN device_event.occurred_at IS 'Čas vzniku události na zařízení';
COMMENT ON COLUMN device_event.idempotency_key IS 'Klíč pro deduplikaci událostí';
COMMENT ON COLUMN device_event.plate_number IS 'Extrahovaná registrační značka z payload (pro index)';
COMMENT ON COLUMN device_event.payload IS 'Kompletní payload události v JSON';
COMMENT ON COLUMN device_event.processing_status IS 'Stav zpracování: RECEIVED, PROCESSING, PROCESSED, FAILED';

-- Partitions pro rok 2026 (leden až prosinec)
CREATE TABLE device_event_2026_01 PARTITION OF device_event
    FOR VALUES FROM ('2026-01-01') TO ('2026-02-01');
CREATE TABLE device_event_2026_02 PARTITION OF device_event
    FOR VALUES FROM ('2026-02-01') TO ('2026-03-01');
CREATE TABLE device_event_2026_03 PARTITION OF device_event
    FOR VALUES FROM ('2026-03-01') TO ('2026-04-01');
CREATE TABLE device_event_2026_04 PARTITION OF device_event
    FOR VALUES FROM ('2026-04-01') TO ('2026-05-01');
CREATE TABLE device_event_2026_05 PARTITION OF device_event
    FOR VALUES FROM ('2026-05-01') TO ('2026-06-01');
CREATE TABLE device_event_2026_06 PARTITION OF device_event
    FOR VALUES FROM ('2026-06-01') TO ('2026-07-01');
CREATE TABLE device_event_2026_07 PARTITION OF device_event
    FOR VALUES FROM ('2026-07-01') TO ('2026-08-01');
CREATE TABLE device_event_2026_08 PARTITION OF device_event
    FOR VALUES FROM ('2026-08-01') TO ('2026-09-01');
CREATE TABLE device_event_2026_09 PARTITION OF device_event
    FOR VALUES FROM ('2026-09-01') TO ('2026-10-01');
CREATE TABLE device_event_2026_10 PARTITION OF device_event
    FOR VALUES FROM ('2026-10-01') TO ('2026-11-01');
CREATE TABLE device_event_2026_11 PARTITION OF device_event
    FOR VALUES FROM ('2026-11-01') TO ('2026-12-01');
CREATE TABLE device_event_2026_12 PARTITION OF device_event
    FOR VALUES FROM ('2026-12-01') TO ('2027-01-01');

-- Výchozí partition pro data mimo rok 2026
CREATE TABLE device_event_default PARTITION OF device_event DEFAULT;

-- =============================================================================
-- 10. app_user — Uživatel aplikace
-- =============================================================================

CREATE TABLE app_user (
    id              UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    external_subject TEXT       NOT NULL UNIQUE,
    display_name    TEXT        NOT NULL,
    email           TEXT,
    role            TEXT        NOT NULL CHECK (role IN ('ADMIN', 'TECHNICIAN', 'FINANCE')),
    is_active       BOOLEAN     NOT NULL DEFAULT true,
    is_deleted      BOOLEAN     NOT NULL DEFAULT false,
    deleted_at      TIMESTAMPTZ,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now()
);

COMMENT ON TABLE app_user IS 'Uživatel aplikace — autentizovaný přes externího poskytovatele identity';
COMMENT ON COLUMN app_user.external_subject IS 'Identifikátor uživatele z externího IdP (subject claim)';
COMMENT ON COLUMN app_user.display_name IS 'Zobrazované jméno uživatele';
COMMENT ON COLUMN app_user.role IS 'Role uživatele: ADMIN, TECHNICIAN, FINANCE';

-- =============================================================================
-- 11. audit_log — Auditní log (partitioned by RANGE on created_at)
-- =============================================================================

CREATE TABLE audit_log (
    id              UUID        NOT NULL DEFAULT gen_random_uuid(),
    app_user_id     UUID,       -- Logický odkaz na app_user (BEZ FK — partitioned tabulka); null pro systémové akce
    action          TEXT        NOT NULL,
    entity_type     TEXT        NOT NULL,
    entity_id       UUID,
    old_values      JSONB,
    new_values      JSONB,
    ip_address      INET,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    PRIMARY KEY (id, created_at)
) PARTITION BY RANGE (created_at);

COMMENT ON TABLE audit_log IS 'Auditní log — záznam všech změn v systému';
COMMENT ON COLUMN audit_log.app_user_id IS 'Uživatel, který provedl akci (null = systémová akce)';
COMMENT ON COLUMN audit_log.action IS 'Typ akce (např. CREATE, UPDATE, DELETE)';
COMMENT ON COLUMN audit_log.entity_type IS 'Typ entity (název tabulky)';
COMMENT ON COLUMN audit_log.entity_id IS 'ID entity, které se akce týká';
COMMENT ON COLUMN audit_log.old_values IS 'Původní hodnoty před změnou (JSON)';
COMMENT ON COLUMN audit_log.new_values IS 'Nové hodnoty po změně (JSON)';
COMMENT ON COLUMN audit_log.ip_address IS 'IP adresa klienta';

-- Partitions pro rok 2026 (leden až prosinec)
CREATE TABLE audit_log_2026_01 PARTITION OF audit_log
    FOR VALUES FROM ('2026-01-01') TO ('2026-02-01');
CREATE TABLE audit_log_2026_02 PARTITION OF audit_log
    FOR VALUES FROM ('2026-02-01') TO ('2026-03-01');
CREATE TABLE audit_log_2026_03 PARTITION OF audit_log
    FOR VALUES FROM ('2026-03-01') TO ('2026-04-01');
CREATE TABLE audit_log_2026_04 PARTITION OF audit_log
    FOR VALUES FROM ('2026-04-01') TO ('2026-05-01');
CREATE TABLE audit_log_2026_05 PARTITION OF audit_log
    FOR VALUES FROM ('2026-05-01') TO ('2026-06-01');
CREATE TABLE audit_log_2026_06 PARTITION OF audit_log
    FOR VALUES FROM ('2026-06-01') TO ('2026-07-01');
CREATE TABLE audit_log_2026_07 PARTITION OF audit_log
    FOR VALUES FROM ('2026-07-01') TO ('2026-08-01');
CREATE TABLE audit_log_2026_08 PARTITION OF audit_log
    FOR VALUES FROM ('2026-08-01') TO ('2026-09-01');
CREATE TABLE audit_log_2026_09 PARTITION OF audit_log
    FOR VALUES FROM ('2026-09-01') TO ('2026-10-01');
CREATE TABLE audit_log_2026_10 PARTITION OF audit_log
    FOR VALUES FROM ('2026-10-01') TO ('2026-11-01');
CREATE TABLE audit_log_2026_11 PARTITION OF audit_log
    FOR VALUES FROM ('2026-11-01') TO ('2026-12-01');
CREATE TABLE audit_log_2026_12 PARTITION OF audit_log
    FOR VALUES FROM ('2026-12-01') TO ('2027-01-01');

-- Výchozí partition pro data mimo rok 2026
CREATE TABLE audit_log_default PARTITION OF audit_log DEFAULT;

-- =============================================================================
-- Indexy
-- =============================================================================

-- Parkovací relace — obsazenost parkoviště
CREATE INDEX ix_session_active
    ON parking_session (parking_lot_id, status, entry_at DESC);

-- Parkovací relace — historie vozidla
CREATE INDEX ix_session_vehicle
    ON parking_session (vehicle_id, entry_at DESC);

-- Události zařízení — historie podle zařízení a času
CREATE INDEX ix_device_event_device_time
    ON device_event (device_id, occurred_at DESC);

-- Události zařízení — vyhledávání podle registrační značky
CREATE INDEX ix_device_event_plate
    ON device_event (plate_number, occurred_at DESC)
    WHERE plate_number IS NOT NULL;

-- Události zařízení — deduplikace
CREATE INDEX ix_device_event_idempotency
    ON device_event (device_id, idempotency_key);

-- Platby — vyhledávání podle reference platební brány
CREATE INDEX ix_payment_provider_ref
    ON payment_intent (provider_ref)
    WHERE provider_ref IS NOT NULL;

-- Platby — historie plateb za relaci
CREATE INDEX ix_payment_session
    ON payment_intent (parking_session_id, created_at DESC);

-- Tarify — aktivní tarify parkoviště
CREATE INDEX ix_tariff_lot_valid
    ON tariff (parking_lot_id, valid_from, valid_to)
    WHERE is_active = true;

-- Auditní log — vyhledávání podle entity
CREATE INDEX ix_audit_log_entity
    ON audit_log (entity_type, entity_id, created_at DESC);

-- Auditní log — vyhledávání podle uživatele
CREATE INDEX ix_audit_log_user
    ON audit_log (app_user_id, created_at DESC)
    WHERE app_user_id IS NOT NULL;

-- GIN indexy pro fulltextové prohledávání JSON
CREATE INDEX ix_device_event_payload_gin
    ON device_event USING GIN (payload);

CREATE INDEX ix_audit_log_old_values_gin
    ON audit_log USING GIN (old_values);

CREATE INDEX ix_audit_log_new_values_gin
    ON audit_log USING GIN (new_values);

-- Partial indexy pro soft-delete — pouze aktivní záznamy
CREATE INDEX ix_parking_lot_active
    ON parking_lot (id) WHERE NOT is_deleted;

CREATE INDEX ix_parking_spot_active
    ON parking_spot (id) WHERE NOT is_deleted;

CREATE INDEX ix_vehicle_active
    ON vehicle (id) WHERE NOT is_deleted;

CREATE INDEX ix_device_active
    ON device (id) WHERE NOT is_deleted;

CREATE INDEX ix_tariff_active
    ON tariff (id) WHERE NOT is_deleted;

CREATE INDEX ix_app_user_active
    ON app_user (id) WHERE NOT is_deleted;

