-- CREATE DATABASE Main CHARACTER SET utf8;
-- use Main;

DROP TABLE IF EXISTS TestQuestion;
DROP TABLE IF EXISTS QuestionsAnswer;
DROP TABLE IF EXISTS UserData;
DROP TABLE IF EXISTS Answer;
DROP TABLE IF EXISTS Question;
DROP TABLE IF EXISTS Sessions;
DROP TABLE IF EXISTS Test;
DROP TABLE IF EXISTS Users;

CREATE TABLE Users (
    id INTEGER NOT NULL AUTO_INCREMENT PRIMARY KEY,
    email VARCHAR(128) NOT NULL,
    passphrase VARCHAR(64) NOT NULL,
    administrator BOOLEAN NOT NULL,
    reset VARCHAR(128) -- hash of reset password token
);

CREATE TABLE Test (
    id INTEGER NOT NULL AUTO_INCREMENT PRIMARY KEY,
    userId INTEGER NOT NULL,
    started DATETIME,
    finished DATETIME,
    created DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    type VARCHAR(2) NOT NULL,
    FOREIGN KEY (userId) REFERENCES Users(id)
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
    arIRDP BOOLEAN NOT NULL DEFAULT FALSE,
    srIRDP BOOLEAN NOT NULL DEFAULT FALSE,
    hrIRDP BOOLEAN NOT NULL DEFAULT FALSE,
    hrPayment BOOLEAN NOT NULL DEFAULT FALSE,
    
    FOREIGN KEY(id) REFERENCES Users(id),
    FOREIGN KEY(ar) REFERENCES Test(id),
    FOREIGN KEY(sr) REFERENCES Test(id),
    FOREIGN KEY(hr) REFERENCES Test(id)    
);

CREATE TABLE Answer (
    id INTEGER PRIMARY KEY,
    correct BOOLEAN NOT NULL,
    answer VARCHAR(1000) NOT NULL
);

CREATE TABLE Question (
    id INTEGER PRIMARY KEY,
    question VARCHAR(1000) NOT NULL,
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

CREATE TABLE Sessions (
    sessionId VARCHAR(256) NOT NULL PRIMARY KEY,
    csrftoken VARCHAR(128) NOT NULL,
    expires DATETIME NOT NULL,
    userId INTEGER NULL,
    testId INTEGER NULL,
    FOREIGN KEY(userId) REFERENCES Users(id)
);
