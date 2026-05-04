# Vertical Slice Builder

When implementing a feature:

1. Create a feature folder:
   - `/Features/{FeatureName}/`

2. Keep feature code together and prefer these files when they add value:
   - `Endpoint.cs`
   - `Request.cs`
   - `Response.cs`
   - `Handler.cs`
   - `Validator.cs`
   - `Tests.cs`

3. Follow CQRS:
   - Commands mutate state
   - Queries return data
   - Do not mix read and write responsibilities in one handler

4. Keep endpoints thin:
   - Accept the request
   - Delegate to the feature handler
   - Return the response

5. Prefer feature-local logic and data access before introducing shared services.

Do not:

- Add logic to shared services unless reuse is clearly necessary
- Mix read and write responsibilities
- Organize new work by technical layer instead of feature
- Create abstractions before the current slice needs them

Use this skill whenever implementing or refactoring application behaviour.
