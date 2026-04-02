CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TABLE "Areas" (
    "Id" uuid NOT NULL,
    "City" character varying(100) NOT NULL,
    "Name" character varying(100) NOT NULL,
    "Latitude" double precision NOT NULL,
    "Longitude" double precision NOT NULL,
    CONSTRAINT "PK_Areas" PRIMARY KEY ("Id")
);

CREATE TABLE "Bookings" (
    "Id" uuid NOT NULL,
    "ConsumerId" uuid NOT NULL,
    "ProviderId" uuid NOT NULL,
    "AreaId" uuid NOT NULL,
    "IdempotencyKey" character varying(100) NOT NULL,
    "VolumeInGallons" integer NOT NULL,
    "PricePerGallonSnapshot" numeric(18,4) NOT NULL,
    "TotalPrice" numeric(18,2) NOT NULL,
    "DeliveryAddress" text NOT NULL,
    "Notes" text,
    "Status" integer NOT NULL,
    "ScheduledFor" timestamp with time zone NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Bookings" PRIMARY KEY ("Id")
);

CREATE TABLE "PriceAlerts" (
    "Id" uuid NOT NULL,
    "ConsumerId" uuid NOT NULL,
    "AreaId" uuid NOT NULL,
    "ThresholdTotalPrice" numeric(18,4) NOT NULL,
    "TargetVolumeInGallons" integer NOT NULL,
    "Direction" integer NOT NULL,
    "IsActive" boolean NOT NULL,
    "LastTriggeredRateId" uuid,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_PriceAlerts" PRIMARY KEY ("Id")
);

CREATE TABLE "ProviderRates" (
    "Id" uuid NOT NULL,
    "ProviderId" uuid NOT NULL,
    "AreaId" uuid NOT NULL,
    "PricePerGallon" numeric(18,4) NOT NULL,
    "EffectiveFrom" timestamp with time zone NOT NULL,
    "EffectiveTo" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_ProviderRates" PRIMARY KEY ("Id")
);

CREATE TABLE "Providers" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "CompanyName" character varying(200) NOT NULL,
    "ContactNumber" character varying(50) NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Providers" PRIMARY KEY ("Id")
);

CREATE TABLE "RefreshTokens" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "TokenHash" character varying(500) NOT NULL,
    "ExpiresAt" timestamp with time zone NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "RevokedAt" timestamp with time zone,
    "ReplacedByTokenHash" text,
    CONSTRAINT "PK_RefreshTokens" PRIMARY KEY ("Id")
);

CREATE TABLE "Users" (
    "Id" uuid NOT NULL,
    "Email" character varying(256) NOT NULL,
    "PasswordHash" text NOT NULL,
    "FullName" character varying(150) NOT NULL,
    "Role" integer NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX "IX_Bookings_ConsumerId_IdempotencyKey" ON "Bookings" ("ConsumerId", "IdempotencyKey");

CREATE INDEX "IX_PriceAlerts_ConsumerId_AreaId" ON "PriceAlerts" ("ConsumerId", "AreaId");

CREATE INDEX "IX_ProviderRates_ProviderId_AreaId_EffectiveTo" ON "ProviderRates" ("ProviderId", "AreaId", "EffectiveTo");

CREATE UNIQUE INDEX "IX_Providers_UserId" ON "Providers" ("UserId");

CREATE UNIQUE INDEX "IX_RefreshTokens_TokenHash" ON "RefreshTokens" ("TokenHash");

CREATE INDEX "IX_RefreshTokens_UserId" ON "RefreshTokens" ("UserId");

CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260402181108_InitialCreate', '10.0.5');

COMMIT;

