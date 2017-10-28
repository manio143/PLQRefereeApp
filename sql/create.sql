CREATE TABLE User (
    id INTEGER PRIMARY KEY,
    email VARCHAR(128) NOT NULL,
    passphrase VARCHAR(64) NOT NULL,
    administrator BOOLEAN NOT NULL,
    reset VARCHAR(128) --hash of reset password token
);

CREATE TABLE Test (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    userId INTEGER NOT NULL,
    started DATETIME,
    finished DATETIME,
    type VARCHAR(2) NOT NULL,
    FOREIGN KEY (userId) REFERENCES User(id)
);

CREATE TABLE UserData (
    id INTEGER PRIMARY KEY,
    name VARCHAR(32) NOT NULL,
    surname VARCHAR(32) NOT NULL,
    team VARCHAR(128) NOT NULL,
    ar INTEGER,
    sr INTEGER,
    hr INTEGER,
    arcooldown DATETIME,
    srcooldown DATETIME,
    hrcooldown DATETIME,
    arIRDP BOOLEAN NOT NULL DEFAULT 0,
    srIRDP BOOLEAN NOT NULL DEFAULT 0,
    hrIRDP BOOLEAN NOT NULL DEFAULT 0,
    hrPayment BOOLEAN NOT NULL DEFAULT 0,
    
    FOREIGN KEY(id) REFERENCES User(id),
    FOREIGN KEY(ar) REFERENCES Test(id),
    FOREIGN KEY(sr) REFERENCES Test(id),
    FOREIGN KEY(hr) REFERENCES Test(id)    
);

CREATE TABLE Answer (
    id INTEGER PRIMARY KEY,
    correct BOOLEAN NOT NULL,
    answer VARCHAR(512) NOT NULL
);

CREATE TABLE Question (
    id INTEGER PRIMARY KEY,
    question VARCHAR(512) NOT NULL,
    information VARCHAR(120) NOT NULL DEFAULT '',
    type VARCHAR(2) NOT NULL
);

CREATE TABLE QuestionsAnswer (
    questionId INTEGER NOT NULL,
    answerId INTEGER NOT NULL,
    PRIMARY KEY (questionId, answerId),
    FOREIGN KEY (questionId) REFERENCES Question(id),
    FOREIGN KEY (answerId) REFERENCES Answer(id)
);

CREATE TABLE TestQuestion (
    testId INTEGER NOT NULL,
    questionId INTEGER NOT NULL,
    answerId INTEGER,
    PRIMARY KEY (testId, questionId),
    FOREIGN KEY (testId) REFERENCES Test(id),
    FOREIGN KEY (questionId) REFERENCES Question(id),
    FOREIGN KEY (answerId) REFERENCES Answer(id)    
);

CREATE TABLE Session (
    sessionId VARCHAR(256) NOT NULL PRIMARY KEY,
    csrftoken VARCHAR(128) NOT NULL,
    expires DATETIME NOT NULL,
    userId INTEGER NULL,
    testId INTEGER NULL,
    FOREIGN KEY(userId) REFERENCES User(id)
);