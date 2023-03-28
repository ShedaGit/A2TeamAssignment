# Web Page Parser

## Overview

The aim of this project is to develop a parser for a website page using HTTP requests. The parser should extract and save data in a MSSQL database table.

### Requirements

- The parser should wait for 10 minutes between each bypass to ensure stable and continuous parser operation.
- The sample size and the amount of delay between requests should be determined based on the specific requirements of the website.
- Stored data must be verified and validated to ensure accuracy and completeness.
- Each record should be checked for availability by key and only new records should be saved.
- The parser should be designed as a console application of .Net Framework 4.8 without using Selenium, WebBrowser, etc.

## Evaluation Criteria

This project should be evaluated based on the following criteria:

1. **Parser code**: The parser code should be simple and straightforward without unnecessary complications like abstractions, interfaces, and asynchrony, auxiliary libraries. Linear execution: sending HTTP request - parsing response - saving data.
2. **Database table**: The database table should have an appropriate field structure and data types.
3. **Interaction with the database**: The layer of interaction with the database should use direct SQL queries instead of EntityFramework and similar libraries.

## Additional Details

- The parser should be developed using C# language.
- The estimated labor input in hours should be provided.

---

## Results

### Database Table Structure

The database table used to store the extracted data have the following structure:

```sql
CREATE TABLE WoodDeals (
   Id INT PRIMARY KEY IDENTITY(1,1),
   SellerName NVARCHAR(255) NULL,
   SellerInn CHAR(12) NOT NULL,
   BuyerName NVARCHAR(255) NULL,
   BuyerInn CHAR(12) NULL,
   WoodVolumeBuyer FLOAT NULL,
   WoodVolumeSeller FLOAT NULL,
   DealDate DATE NULL,
   DealNumber NVARCHAR(50) NOT NULL,
   UNIQUE(DealNumber, SellerINN, BuyerINN)
);
```

The table has a primary key `Id` which is an identity column. The `DealNumber`, `SellerINN`, and `BuyerINN` columns form a unique constraint to ensure that only new records are saved. The data types for each column have been selected based on the specific requirements of the project.

### Labor Input

I was able to complete the parser for the website page in 10-12 hours.

I also took care to ensure that the code is of high quality and follows best practices for readability and performance.

### Request Justification

- A good rule of thumb is to wait at least 5-10 seconds between each request to avoid overwhelming the website with too many requests at once. So a delay of 10 seconds between requests may be appropriate.
- In this case, since the website has up to 7,000 pages, and only 20 rows per page, we may want to consider increasing rows per page to 100 to decrease request from website's server or exceeding any API limits.

### Validation Justification

- Declaration number consists of 28 digits, in case the number will contain more digits in the future, allocated a limit of 50.
- The names of the seller and the buyer can be any, so just check that they are not empty and do not exceed 255 characters.
- INN is checked for validity according to the appropriate algorithm for checking INN.
- Transaction date should match the date format and not be in the "future".
- Volumes are only checked for consistency with float number.
