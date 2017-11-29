# Referee Testing Environment for the Polish Quidditch League

This is the source code for the PLQ's referee testing portal which you can visit here: <https://ref.polskaligaquidditcha.pl>

It's a ASP.NET Core MVC application (used to be in F# with Suave), made with security in mind. If you find any bugs, please submit an Issue, describing when, what happens.

## The core functionalities are there

- [x] User can login
- [x] User can register
- [x] User can take a test
- [x] User can viewtheir profile
- [x] User can edit their profile
- [x] User can browse the referee catalog
- [ ] Admin has an easier life managing tests and users - can be achieved by simply editing the database with phpMyAdmin or similar

## Screenshots
Index page

![Index page](screenshots/screen-index.png)

Referee directory

![Directory](screenshots/screen-directory.png)

Register page

![Register](screenshots/screen-register.png)

Profile page

![Profile](screenshots/screen-profile.png)

SR test - user can take the test

![SR](screenshots/screen-sr.png)

AR test - user is in the cooldown period

![AR](screenshots/screen-ar.png)

Starting a test

![Starting test](screenshots/screen-start-test.png)

Taking a test

![Taking test](screenshots/screen-take-test.png)
