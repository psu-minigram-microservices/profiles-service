CREATE TYPE tStatus AS ENUM ('none', 'friend', 'blocked');

CREATE TABLE Profiles(
  Id UUID PRIMARY KEY,
  UserId UUID NOT NULL,
  Name VARCHAR(120) NOT NULL,
  PhotoUrl TEXT,
  CreatedAt TIMESTAMP,
  UpdatedAt TIMESTAMP);

CREATE TABLE Relations(
  Id UUID PRIMARY KEY,
  SenderId UUID NOT NULL,
  ReceiverId UUID NOT NULL,
  Status tStatus NOT NULL,
  CreatedAt TIMESTAMP,
  UpdatedAt TIMESTAMP);

ALTER TABLE Profiles ADD CONSTRAINT UqProfilesUserId UNIQUE (UserId);
ALTER TABLE Relations ADD CONSTRAINT UqRelations UNIQUE (SenderId, ReceiverId);

ALTER TABLE Relations ADD CONSTRAINT FkRelationsSenderId FOREIGN KEY (SenderId) REFERENCES Profiles(Id) ON DELETE CASCADE;
ALTER TABLE Relations ADD CONSTRAINT FkRelationsReceiverId FOREIGN KEY (ReceiverId) REFERENCES Profiles(Id) ON DELETE CASCADE;

ALTER TABLE Relations ALTER COLUMN Status SET DEFAULT 'none';

CREATE INDEX IdxRelationsSenderId ON Relations(SenderId);
CREATE INDEX IdxRelationsReceiverId ON Relations(ReceiverId);
CREATE INDEX IdxRelationsStatus ON Relations(Status);
