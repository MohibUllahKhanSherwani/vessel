START TRANSACTION;
DROP INDEX "IX_ProviderRates_ProviderId_AreaId_EffectiveTo";

CREATE UNIQUE INDEX "IX_ProviderRates_ProviderId_AreaId" ON "ProviderRates" ("ProviderId", "AreaId") WHERE "EffectiveTo" IS NULL;

CREATE UNIQUE INDEX "IX_Areas_City_Name" ON "Areas" ("City", "Name");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260402184538_AddCompositeConstraints', '10.0.5');

COMMIT;

