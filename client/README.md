# GIF Finder (Angular client)

Frontend of the Giphy Integration API. The design is described in the [root README](../README.md#frontend-angular-client).

```bash
npm ci                      # install the exact versions from package-lock.json
npm start                   # http://localhost:4200, expects the API on http://localhost:5006
npm test -- --watch=false   # unit tests (Vitest)
npm run build               # production build in dist/
```

The API URL is set in [src/environments](src/environments).
