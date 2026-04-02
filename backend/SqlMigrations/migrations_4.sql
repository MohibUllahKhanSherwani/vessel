START TRANSACTION;
INSERT INTO "Areas" ("Id", "City", "CreatedAt", "Latitude", "Longitude", "Name", "UpdatedAt")
VALUES ('20efca73-66e5-6f6f-87cc-6245405bdd62', 'Karachi', TIMESTAMPTZ '2026-04-01T00:00:00+00:00', 24.8064, 67.030100000000004, 'Clifton Block 4', TIMESTAMPTZ '2026-04-01T00:00:00+00:00');
INSERT INTO "Areas" ("Id", "City", "CreatedAt", "Latitude", "Longitude", "Name", "UpdatedAt")
VALUES ('3a3f69c7-13c9-9a81-52d6-9940e12e7752', 'Karachi', TIMESTAMPTZ '2026-04-01T00:00:00+00:00', 24.916699999999999, 67.083299999999994, 'Gulshan-e-Iqbal', TIMESTAMPTZ '2026-04-01T00:00:00+00:00');
INSERT INTO "Areas" ("Id", "City", "CreatedAt", "Latitude", "Longitude", "Name", "UpdatedAt")
VALUES ('57e9f6aa-d280-db23-9ba7-c7e91dcd4fb3', 'Islamabad', TIMESTAMPTZ '2026-04-01T00:00:00+00:00', 33.729399999999998, 73.093199999999996, 'F-6 Markaz', TIMESTAMPTZ '2026-04-01T00:00:00+00:00');
INSERT INTO "Areas" ("Id", "City", "CreatedAt", "Latitude", "Longitude", "Name", "UpdatedAt")
VALUES ('7a63151e-a977-efa9-d57a-ab1612e6a93e', 'Lahore', TIMESTAMPTZ '2026-04-01T00:00:00+00:00', 31.506, 74.355599999999995, 'Gulberg III', TIMESTAMPTZ '2026-04-01T00:00:00+00:00');
INSERT INTO "Areas" ("Id", "City", "CreatedAt", "Latitude", "Longitude", "Name", "UpdatedAt")
VALUES ('8019e41f-6ad6-a993-79a4-d146e81d1d5d', 'Islamabad', TIMESTAMPTZ '2026-04-01T00:00:00+00:00', 33.6952, 73.012900000000002, 'F-10 Markaz', TIMESTAMPTZ '2026-04-01T00:00:00+00:00');
INSERT INTO "Areas" ("Id", "City", "CreatedAt", "Latitude", "Longitude", "Name", "UpdatedAt")
VALUES ('83dfdacb-eae8-6d60-eddd-0d8cae0b20bb', 'Lahore', TIMESTAMPTZ '2026-04-01T00:00:00+00:00', 31.484400000000001, 74.324399999999997, 'Model Town', TIMESTAMPTZ '2026-04-01T00:00:00+00:00');
INSERT INTO "Areas" ("Id", "City", "CreatedAt", "Latitude", "Longitude", "Name", "UpdatedAt")
VALUES ('93c8ebd6-4324-e55d-4c12-9c3fffc23554', 'Karachi', TIMESTAMPTZ '2026-04-01T00:00:00+00:00', 24.796700000000001, 67.049499999999995, 'DHA Phase 6', TIMESTAMPTZ '2026-04-01T00:00:00+00:00');
INSERT INTO "Areas" ("Id", "City", "CreatedAt", "Latitude", "Longitude", "Name", "UpdatedAt")
VALUES ('e5bb14b7-bfcd-46b1-903c-5a0d678cdd69', 'Lahore', TIMESTAMPTZ '2026-04-01T00:00:00+00:00', 31.494499999999999, 74.353399999999993, 'DHA Phase 6', TIMESTAMPTZ '2026-04-01T00:00:00+00:00');
INSERT INTO "Areas" ("Id", "City", "CreatedAt", "Latitude", "Longitude", "Name", "UpdatedAt")
VALUES ('ebf6fb5d-aa84-47ca-53f3-b723656d6758', 'Islamabad', TIMESTAMPTZ '2026-04-01T00:00:00+00:00', 33.713299999999997, 73.061899999999994, 'Blue Area', TIMESTAMPTZ '2026-04-01T00:00:00+00:00');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260402191935_SeedStarterAreas', '10.0.5');

COMMIT;

