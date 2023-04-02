import ApiAuthorzationRoutes from './components/api-authorization/ApiAuthorizationRoutes';
import { Counter } from "./components/Counter";
import { FetchData } from "./components/FetchData";
import FilmsList from './components/FilmsList';
import { Home } from "./components/Home";
import Player from './components/Player';

const AppRoutes = [
    {
        index: true,
        element: <Home />
    },
    {
        path: '/counter',
        element: <Counter />
    },
    {
        path: '/fetch-data',
        requireAuth: true,
        element: <FetchData />
    },
    {
        path: '/films-list',
        element: <FilmsList />
    },
    {
        path: '/player/:title',
        requireAuth: true,
        element: <Player />
    },
  ...ApiAuthorzationRoutes
];

export default AppRoutes;
