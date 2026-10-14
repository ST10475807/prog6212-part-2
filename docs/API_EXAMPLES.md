# RaceDay API — example request bodies

Use these examples in Swagger (`http://localhost:5080/swagger`). The database must have been created using the EF Core migration steps in the README first.

## 1. Register an account — `POST /api/auth/register`

Organizer example:

```json
{
  "firstName": "Alex",
  "lastName": "Mokoena",
  "email": "alex.organizer@example.com",
  "password": "StrongPass123!",
  "role": "Organizer",
  "phone": "0712345678"
}
```

Participant example: use a different email and set `role` to `Participant`.

## 2. Login — `POST /api/auth/login`

```json
{
  "email": "alex.organizer@example.com",
  "password": "StrongPass123!"
}
```

Copy the `accessToken` value from the response, click **Authorize** in Swagger, and paste the token. Do not paste the word `Bearer` unless the dialog explicitly asks for it; the configured Swagger dialog adds the bearer scheme.

## 3. Create an event — `POST /api/events`

Authorize using an Organizer token first.

```json
{
  "eventName": "Johannesburg Spring Run",
  "description": "Community running event.",
  "eventDate": "2027-03-20T00:00:00",
  "startTime": "08:00:00",
  "venue": "Athletics Stadium",
  "location": "Johannesburg, South Africa",
  "maxParticipants": 250,
  "status": "Open",
  "eventType": "Run"
}
```

Allowed event types: `Run`, `Walk`, `Cycle`. Allowed statuses: `Draft`, `Open`, `Closed`, `Cancelled`, `Completed`.

Optional list filters can be supplied to `GET /api/events`, for example:

`/api/events?status=Open&eventType=Run&fromDate=2027-01-01&toDate=2027-12-31`

## 4. Create a category — `POST /api/categories`

Authorize using an Organizer token.

```json
{
  "categoryName": "10K Road Race",
  "description": "A ten kilometre road-running category.",
  "minAge": 16,
  "maxAge": 120,
  "distanceKm": 10
}
```

## 5. Assign a category to an event — `POST /api/events/{eventId}/categories/{categoryId}`

Replace the route IDs with the IDs returned by the event and category creation requests.

```json
{
  "maxParticipants": 100,
  "entryFee": 150.00
}
```

## 6. Enrol a participant

Log in as a Participant, authorize with that token, and call:

`POST /api/events/{eventId}/categories/{categoryId}/enrolments`

No request body is needed. The event must have status `Open` and the category must be assigned to it.

## 7. Record a result — `POST /api/enrolments/{enrolmentId}/result`

Authorize using the Organizer token that owns the event.

```json
{
  "finishPosition": 1,
  "finishTime": "00:42:15",
  "resultStatus": "Finished"
}
```

Allowed result statuses: `Pending`, `Finished`, `DNF`, `DNS`.
