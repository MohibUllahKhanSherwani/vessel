START TRANSACTION;
ALTER TABLE "RefreshTokens" ADD "UpdatedAt" timestamp with time zone NOT NULL DEFAULT TIMESTAMPTZ '-infinity';

ALTER TABLE "ProviderRates" ADD "UpdatedAt" timestamp with time zone NOT NULL DEFAULT TIMESTAMPTZ '-infinity';

ALTER TABLE "Areas" ADD "CreatedAt" timestamp with time zone NOT NULL DEFAULT TIMESTAMPTZ '-infinity';

ALTER TABLE "Areas" ADD "UpdatedAt" timestamp with time zone NOT NULL DEFAULT TIMESTAMPTZ '-infinity';

CREATE INDEX "IX_ProviderRates_AreaId" ON "ProviderRates" ("AreaId");

CREATE INDEX "IX_PriceAlerts_AreaId" ON "PriceAlerts" ("AreaId");

CREATE INDEX "IX_Bookings_AreaId" ON "Bookings" ("AreaId");

CREATE INDEX "IX_Bookings_ProviderId" ON "Bookings" ("ProviderId");

ALTER TABLE "Bookings" ADD CONSTRAINT "FK_Bookings_Areas_AreaId" FOREIGN KEY ("AreaId") REFERENCES "Areas" ("Id") ON DELETE RESTRICT;

ALTER TABLE "Bookings" ADD CONSTRAINT "FK_Bookings_Providers_ProviderId" FOREIGN KEY ("ProviderId") REFERENCES "Providers" ("Id") ON DELETE RESTRICT;

ALTER TABLE "Bookings" ADD CONSTRAINT "FK_Bookings_Users_ConsumerId" FOREIGN KEY ("ConsumerId") REFERENCES "Users" ("Id") ON DELETE RESTRICT;

ALTER TABLE "PriceAlerts" ADD CONSTRAINT "FK_PriceAlerts_Areas_AreaId" FOREIGN KEY ("AreaId") REFERENCES "Areas" ("Id") ON DELETE RESTRICT;

ALTER TABLE "PriceAlerts" ADD CONSTRAINT "FK_PriceAlerts_Users_ConsumerId" FOREIGN KEY ("ConsumerId") REFERENCES "Users" ("Id") ON DELETE RESTRICT;

ALTER TABLE "ProviderRates" ADD CONSTRAINT "FK_ProviderRates_Areas_AreaId" FOREIGN KEY ("AreaId") REFERENCES "Areas" ("Id") ON DELETE RESTRICT;

ALTER TABLE "ProviderRates" ADD CONSTRAINT "FK_ProviderRates_Providers_ProviderId" FOREIGN KEY ("ProviderId") REFERENCES "Providers" ("Id") ON DELETE RESTRICT;

ALTER TABLE "RefreshTokens" ADD CONSTRAINT "FK_RefreshTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260402185243_UpdateAuditAndDeletes', '10.0.5');

COMMIT;

