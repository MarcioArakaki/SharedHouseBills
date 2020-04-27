CREATE TABLE Bill (
    Id int PRIMARY KEY IDENTITY (1, 1),
    BillTypeId int,
    Value float NOT NULL,
    PaymentDate Datetime2,
    DueDate Datetime2,
    CONSTRAINT FK_Bill_BillType FOREIGN KEY (BillTypeId)
    REFERENCES BillType(Id)
);