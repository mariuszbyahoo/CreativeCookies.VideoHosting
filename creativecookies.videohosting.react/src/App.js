import { BrowserRouter, Route, Routes } from "react-router-dom";
import "./App.css";
import Navbar from "./Components/Navbar";
import Home from "./Components/Home";
import ErrorBoundary from "./Components/ErrorBoundary";
import Player from "./Components/Player";

function App() {
  return (
    <BrowserRouter>
      <Navbar />
      <ErrorBoundary>
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/player/:title" element={<Player />} />
        </Routes>
      </ErrorBoundary>
    </BrowserRouter>
  );
}

export default App;
