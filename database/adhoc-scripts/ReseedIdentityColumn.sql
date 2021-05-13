/*
    Identify all user objects and create drop
*/

dbcc checkident ('Schema.Table', reseed, 0);