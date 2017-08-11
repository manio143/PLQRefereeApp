CREATE TABLE User (
    id INT NOT NULL PRIMARY KEY,
    email VARCHAR(128) NOT NULL,
    passphrase VARCHAR(64) NOT NULL,
    reset VARCHAR(128) --hash of reset password token
)

CREATE TABLE Session (
    sessionId VARCHAR(256) NOT NULL PRIMARY KEY,
    csrftoken VARCHAR(128) NOT NULL,
    expires DATETIME NOT NULL,
    userId INT NULL,
    testId INT NULL,
    FOREIGN KEY(userId) REFERENCES User(id)
)