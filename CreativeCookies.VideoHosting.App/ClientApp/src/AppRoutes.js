import ApiAuthorzationRoutes from "./components/api-authorization/ApiAuthorizationRoutes";
import FilmsList from "./components/FilmsList";
import FilmUpload from "./components/FilmUpload";
import { Home } from "./components/Home";
import Player from "./components/Player";

const AppRoutes = [
  {
    index: true,
    element: <Home />,
  },
  {
    path: "/films-list",
    element: <FilmsList />,
  },
  {
    path: "/player/:title",
    requireAuth: true,
    element: <Player />,
  },
  {
    path: "/films-upload",
    requireAuth: true,
    element: <FilmUpload />,
  },
  ...ApiAuthorzationRoutes,
];

export default AppRoutes;
