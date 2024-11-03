### What is Pagination?

It is a technique used to divide a large dataset into smaller chunks (or "pages") and retrieve these chunks incrementally rather than all at once. 

This allows users to view data in segments instead of loading an entire data set in a single operation.

---
#### Offset

> Did you know pagination with `offset` is very troublesome but easy to avoid?
> -- <cite>Markus Winand</cite>

> Skip is evil in various ways; it's like `Sleep()` ... it makes your program pause/wait the bigger it is.

##### Basic definition:
This method uses a starting index (offset) and a count (limit) to specify which records to return.

Example in c# 
```csharp
// goes to the 3rd page (with page size equals 10)
// returns 10 records starting from the 21st record
var someBlogs = await ctx.Blogs  
    .OrderBy(b => b.Id)  
    .Skip(20)  
    .Take(10) 
    .ToArrayAsync();
```

Actual query (in PostgreSQL)
```sql
-- returns 10 records starting from the 21st record
SELECT b."Id", b."LastUpdated", b."Name"
FROM "Blogs" AS b
ORDER BY b."Id"
LIMIT @__p_1 OFFSET @__p_0
```

##### Problems
- Can become inefficient on very large datasets as the database must scan through the records to reach the specified offset, especially for high offset (example case: when user have access to random page in large dataset; go to the last page)
- Concurrent db modifications - in case there was an insert in db just before user goes to the next page, user can see the same entry again (pages are 'defined' dynamically depending on the number of entries); same case when row/s are deleted.
- can access data/pages only based on arbitrary values, page size

--- 
#### Keyset

aka *Seek based pagination*
aka *Cursor based pagination*
##### Basic definition
This approach relies on the value of a unique or indexed column (usually an ID or timestamp/data) to determine the starting point for the next page.

Example in c#
```csharp
var someBlogs = await ctx.Blogs  
    .OrderBy(b => b.Id)  
    .Where(b => b.Id > 20) // uses index
    .Take(10) // page size  
    .ToArrayAsync();
```

Query in PostgreSQL
```sql
SELECT b."Id", b."LastUpdated", b."Name"
FROM "Blogs" AS b
WHERE b."Id" > 20
ORDER BY b."Id"
LIMIT @__p_0
```

##### Problems
- More complex to implement (especially in cases when you have to paginate by using multiple columns), and not as flexible for accessing arbitrary pages (doesn't allow random access), as it’s generally limited to navigating forward or backward one page at a time.
##### Use cases
- infinite scrolling (for example git commits, any type of feed)
- you can still use pages to jump to arbitrary value depending on the business requirement, for example if you divide pages by `Name` you should be able to skip to page starting with `A`, `B` and so on.

---
### Benchmarks

![pagination-benchmarks.png](https://github.com/mirusser/Pagination/blob/main/Pictures/pagination-benchmarks.png?raw=true)
#### Key Takeaways
- **Offset Pagination**: Fast for small offsets but scales poorly with larger datasets.
- **Keyset Pagination**: Performs better with large datasets, as it doesn’t require scanning and skipping through rows.
---
### Graph

![offset-seek-graph.png](https://github.com/mirusser/Pagination/blob/main/Pictures/offset-seek-graph.png?raw=true)

If you wanna do any type pagination you wanna have well defined ordering (having an index/es on column/s), otherwise the performance gonna take a big hit.

### Sources:
- [no-offset - manifesto](https://use-the-index-luke.com/no-offset)
- [MR.EntityFrameworkCore.KeysetPagination](https://github.com/mrahhal/MR.EntityFrameworkCore.KeysetPagination)
- [.NET Standup session - Database Pagination](https://www.youtube.com/watch?v=DIKH-q-gJNU)
- [ms docs - pagination in ef core](https://learn.microsoft.com/en-us/ef/core/querying/pagination)
-  [GraphQL workshop - Adding paging to your lists](https://github.com/ChilliCream/graphql-workshop/blob/main/docs/5-adding-complex-filter-capabilities.md#adding-paging-to-your-lists)
---

