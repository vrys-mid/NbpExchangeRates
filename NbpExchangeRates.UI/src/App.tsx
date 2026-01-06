import { Routes, Route, Link } from "react-router-dom";
import { HomePage } from "./pages/HomePage";
import { CurrencyDetailsPage } from "./pages/CurrencyDetailsPage";
import { CurrencyConverterPage } from "./pages/CurrencyConverterPage";

function App() {
  return (
    <div className="min-h-screen bg-gray-100 p-6">
      <div className="max-w-6xl mx-auto">
        <h1 className="text-3xl font-bold mb-4">
          NBP – Table B Exchange Rates
        </h1>
        
        <nav className="mb-6 flex gap-6 border-b border-gray-300 pb-2">
          <Link 
            to="/" 
            className="text-blue-600 hover:text-blue-800 hover:underline font-medium"
          >
            Exchange Rates
          </Link>
          <Link 
            to="/converter" 
            className="text-blue-600 hover:text-blue-800 hover:underline font-medium"
          >
            Currency Converter
          </Link>
        </nav>

        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/currency/:code" element={<CurrencyDetailsPage />} />
          <Route path="/converter" element={<CurrencyConverterPage />} />
        </Routes>
      </div>
    </div>
  );
}

export default App;
