# Manual Swagger verification checklist

Use test accounts created via `/api/auth/register`; do not use the demo password-hash strings from the Part 1 SQL inserts for login.

1. Register Organizer and Participant accounts.
2. Attempt registration with an invalid role; expect HTTP 400.
3. Log in with valid credentials; expect HTTP 200 and `accessToken`.
4. Log in with a wrong password; expect HTTP 401.
5. Call `GET /api/users/me` without a token; expect HTTP 401.
6. Authorize Swagger with the Participant token and try `POST /api/events`; expect HTTP 403.
7. Authorize Swagger with the Organizer token and create an event; expect HTTP 201.
8. Attempt to update/delete another Organizer's event; expect HTTP 403.
9. Create category and assign it to an owned event.
10. Enrol as Participant in an open event category; expect HTTP 201.
11. Try to enrol twice in the same category; expect HTTP 409.
12. Confirm Organizer can list enrolments only for their own event.
13. Record a result as the event's Organizer.
14. Confirm Participant can read their own result but not another participant's result.
15. Run `dotnet test RaceDay.sln` and verify the GitHub Actions workflow after pushing.
