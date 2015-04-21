For the database configuration we stuck to the same kind of configuration as the specs

It made most sense to have a Players, Words, and Games table

Players: pid, playerName
Games: gId, p1Id, p2Id, p1Score, p2Score, gameEnded, timeLimit, board
Words: gId, pId, word

We used a bunch of basic queries using select statements with where statements to find equality

The inserts were all straight foward