import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { fetchLatestRates } from "../api/ratesApi";
import * as emojiFlags from "country-currency-emoji-flags";

export const CurrencyConverterPage = () => {
  const [fromCurrency, setFromCurrency] = useState("");
  const [toCurrency, setToCurrency] = useState("");
  const [amount, setAmount] = useState("1");

  const { data: rates, isLoading } = useQuery({
    queryKey: ["rates", "latest"],
    queryFn: fetchLatestRates,
  });

  if (isLoading) return <p>Loading currencies...</p>;

  const calculateConversion = () => {
    if (!rates || !fromCurrency || !toCurrency || !amount) return null;

    const fromRate = rates.find((r) => r.code === fromCurrency);
    const toRate = rates.find((r) => r.code === toCurrency);

    if (!fromRate || !toRate) return null;

    const amountNum = parseFloat(amount);
    const plnAmount = amountNum * fromRate.mid;
    const convertedAmount = plnAmount / toRate.mid;

    return {
      rate: fromRate.mid / toRate.mid,
      convertedAmount,
    };
  };

  const result = calculateConversion();

  return (
    <div className="currency-converter-container">
      <div className="bg-white p-8 rounded shadow">
        <h2 className="text-2xl font-bold mb-6">Currency Converter</h2>

        <div className="space-y-6">
<div>
  <label className="block text-sm font-medium text-gray-700 mb-2">
    From
  </label>
  <div className="flex items-center gap-4">
    <input
      type="number"
      value={amount}
      onChange={(e) => setAmount(e.target.value)}
      className="w-32 border rounded px-4 py-2 focus:ring-2 focus:ring-blue-500 outline-none"
      placeholder="Amount"
      min="0"
      step="any"
    />
    
    <select
      value={fromCurrency}
      onChange={(e) => setFromCurrency(e.target.value)}
      className="border rounded px-4 py-2 focus:ring-2 focus:ring-blue-500 outline-none"
    >
      <option value="">Select currency</option>
      {rates?.map((r) => (
        <option key={r.code} value={r.code}>
          {emojiFlags.getEmojiByCurrencyCode(r.code) || ""} {r.code} - {r.currency}
        </option>
      ))}
    </select>
  </div>
</div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              To
            </label>
            <select
              value={toCurrency}
              onChange={(e) => setToCurrency(e.target.value)}
              className="w-full border rounded px-4 py-2"
            >
              <option value="">Select currency</option>
              {rates?.map((r) => (
                <option key={r.code} value={r.code}>
                  {emojiFlags.getEmojiByCurrencyCode(r.code) || ""} {r.code} - {r.currency}
                </option>
              ))}
            </select>
          </div>

          {result && (
            <div className="mt-6 p-6 bg-blue-50 rounded">
              <div className="text-center">

                <p className="text-sm text-gray-600 mt-2">
                  {amount} {fromCurrency} = {result.convertedAmount.toFixed(4)} {toCurrency}
                </p>
                <p className="text-xs text-gray-500 mt-2">
                  Exchange rate: 1 {fromCurrency} = {result.rate.toFixed(6)} {toCurrency}
                </p>
              </div>
            </div>
          )}

          {fromCurrency && toCurrency && !result && (
            <div className="mt-6 p-4 bg-yellow-50 rounded text-center text-sm text-gray-600">
              Please enter a valid amount
            </div>
          )}
        </div>
      </div>
    </div>
  );
};