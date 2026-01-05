import type { CurrencyRate } from "../types/CurrencyRate";
import { formatDate } from "../utils/date";
import { useNavigate } from "react-router-dom";
import * as emojiFlags from "country-currency-emoji-flags";

interface Props {
  rates: CurrencyRate[];
  onSort: (field: "code" | "currency" | "mid") => void;
  sortField: string;
  sortDirection: string;
}

export const RatesTable = ({ rates, onSort, sortField, sortDirection }: Props) => {
  const sortIcon = (field: "code" | "currency" | "mid") => {
    if (sortField !== field) return null;
    return sortDirection === "asc" ? " ▲" : " ▼";
  };
  
  const navigate = useNavigate();
  
  const getFlag = (currencyCode: string) => {
    return emojiFlags.getEmojiByCurrencyCode(currencyCode) || "🏳️";
  };
  
  return (
    <div className="overflow-x-auto">
      <table className="min-w-full text-sm border-collapse">
        <thead className="text-gray-600 uppercase text-xs">
          <tr>
            <th className="px-6 py-3 text-left">Flag</th>
            <th
              onClick={() => onSort("code")}
              className="cursor-pointer px-6 py-3 text-left"
            >
              Code{sortIcon("code")}
            </th>
            <th
              onClick={() => onSort("currency")}
              className="cursor-pointer px-6 py-3 text-left"
            >
              Currency{sortIcon("currency")}
            </th>
            <th
              onClick={() => onSort("mid")}
              className="cursor-pointer px-6 py-3 text-right"
            >
              Rate{sortIcon("mid")}
            </th>
            <th className="px-6 py-3 text-left">Date</th>
            <th className="px-6 py-3 text-center">Actions</th>
          </tr>
        </thead>
        <tbody>
          {rates.map((r, index) => (
            <tr
              key={`${r.code}-${r.effectiveDate}`}
            >
              <td className="px-6 py-4 text-2xl">
                {getFlag(r.code)}
              </td>
              <td className="px-6 py-4 font-medium">
                {r.code}
              </td>
              <td className="px-6 py-4">{r.currency}</td>
              <td className="px-6 py-4 text-right font-mono">
                {r.mid.toFixed(4)}
              </td>
              <td className="px-6 py-4 text-gray-600">
                {formatDate(r.effectiveDate)}
              </td>
              <td className="px-6 py-4 text-center">
                <button
                  onClick={() => navigate(`/currency/${r.code}`)}
                  className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 transition-colors"
                >
                  History
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};