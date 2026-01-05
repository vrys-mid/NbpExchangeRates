import { Routes, Route } from "react-router-dom";
import { HomePage } from "./pages/HomePage";
import { CurrencyDetailsPage } from "./pages/CurrencyDetailsPage";

function App() {
  return (
    <div className="min-h-screen bg-gray-100 p-6">
      <div className="max-w-6xl mx-auto">
        <h1 className="text-3xl font-bold mb-6">
          NBP – Table B Exchange Rates
        </h1>

        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/currency/:code" element={<CurrencyDetailsPage />} />
        </Routes>
      </div>
    </div>
  );
}

export default App;
