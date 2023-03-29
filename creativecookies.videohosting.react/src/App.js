import { BrowserRouter } from "react-router-dom";
import "./App.css";
import Navbar from "./Components/Navbar";
import Home from "./Components/Home";
import ErrorBoundary from "./Components/ErrorBoundary";

function App() {
  return (
    <BrowserRouter>
      <Navbar />
      <ErrorBoundary>
        <Home />
      </ErrorBoundary>
    </BrowserRouter>
  );
}

export default App;
