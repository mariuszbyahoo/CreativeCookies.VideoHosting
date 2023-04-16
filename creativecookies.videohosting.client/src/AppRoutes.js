import { ErrorBoundary } from "react-error-boundary";
import FilmsList from "./components/FilmsList";
import FilmUpload from "./components/FilmUpload";
import { Home } from "./components/Home";
import Player from "./components/Player";

function fallbackRender({ error, resetErrorBoundary }) {
  // Call resetErrorBoundary() to reset the error boundary and retry the render.
  let errorSendingStatus = "sending...";

  fetch(`https://${process.env.REACT_APP_API_ADDRESS}/api/error/`, {
    method: "POST",
    mode: "cors",
    cache: "no-cache",
    credentials: "same-origin",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      errorLog: JSON.stringify({
        error: {
          message: error.message,
          stack: error.stack,
        },
      }),
    }),
  })
    .then((errorSendingStatus = "Error sent, contact the site Admin"))
    .catch((error) => {
      let strError = JSON.stringify({
        error: {
          message: error.message,
          stack: error.stack,
        },
      });
      console.log(
        "Error when sending the error message to the API: ",
        strError
      );
    });
  return (
    <div role="alert">
      <p>Something went wrong:</p>
      <pre style={{ color: "red" }}>{error.message}</pre>
      <p>{errorSendingStatus}</p>
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
    requireAuth: false,
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
    requireAuth: false,
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
];

export default AppRoutes;
