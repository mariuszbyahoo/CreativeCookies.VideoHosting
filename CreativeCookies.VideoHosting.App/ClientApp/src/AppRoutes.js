import ApiAuthorzationRoutes from "./components/api-authorization/ApiAuthorizationRoutes";
import FilmsList from "./components/FilmsList";
import FilmUpload from "./components/FilmUpload";
import { Home } from "./components/Home";
import Player from "./components/Player";
import { ErrorBoundary } from "react-error-boundary";

function fallbackRender({ error, resetErrorBoundary }) {
  // Call resetErrorBoundary() to reset the error boundary and retry the render.

  return (
    <div role="alert">
      <p>Something went wrong:</p>
      <pre style={{ color: "red" }}>{error.message}</pre>
    </div>
  );
}
const AppRoutes = [
  {
    index: true,
    element: (
      <ErrorBoundary
        fallbackRender={fallbackRender}
        onReset={(details) => {
          console.log("ErrorBoundary onReset: ", details);
        }}
      >
        <Home />
      </ErrorBoundary>
    ),
  },
  {
    path: "/films-list",
    element: (
      <ErrorBoundary
        fallbackRender={fallbackRender}
        onReset={(details) => {
          console.log("ErrorBoundary onReset: ", details);
        }}
      >
        <FilmsList />
      </ErrorBoundary>
    ),
  },
  {
    path: "/player/:title",
    requireAuth: true,
    element: (
      <ErrorBoundary
        fallbackRender={fallbackRender}
        onReset={(details) => {
          console.log("ErrorBoundary onReset: ", details);
        }}
      >
        <Player />
      </ErrorBoundary>
    ),
  },
  {
    path: "/films-upload",
    requireAuth: true,
    element: (
      <ErrorBoundary
        fallbackRender={fallbackRender}
        onReset={(details) => {
          console.log("ErrorBoundary onReset: ", details);
        }}
      >
        <FilmUpload />
      </ErrorBoundary>
    ),
  },
  ...ApiAuthorzationRoutes,
];

export default AppRoutes;
