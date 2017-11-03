ALTER TABLE `TestQuestion`
DROP FOREIGN KEY `TestQuestion_ibfk_1`;

ALTER TABLE `TestQuestion`
ADD CONSTRAINT `TestQuestion_ibfk_1` FOREIGN KEY (`testId`) REFERENCES `Test`(`id`)
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE `TestQuestion`
DROP FOREIGN KEY `TestQuestion_ibfk_2`;

ALTER TABLE `TestQuestion`
ADD CONSTRAINT `TestQuestion_ibfk_2` FOREIGN KEY (`questionId`) REFERENCES `Question`(`id`)
  ON DELETE CASCADE
  ON UPDATE CASCADE;
ALTER TABLE `TestQuestion`
DROP FOREIGN KEY `TestQuestion_ibfk_3`;

ALTER TABLE `TestQuestion`
ADD CONSTRAINT `TestQuestion_ibfk_3` FOREIGN KEY (`answerId`) REFERENCES `Answer`(`id`)
  ON DELETE CASCADE
  ON UPDATE CASCADE;
