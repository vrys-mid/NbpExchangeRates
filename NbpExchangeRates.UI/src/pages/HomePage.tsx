import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { fetchRatesByDate, fetchPublicationDates } from "../api/ratesApi";
import { RatesTable } from "../components/RatesTable";
import type { CurrencyRate } from "../types/CurrencyRate";
import { useFavorites } from "../hooks/useFavorites";

type SortField = "code" | "currency" | "mid";
type SortDirection = "asc" | "desc";

export const HomePage = () => {
  const [query, setQuery] = useState("");
  const [selectedDate, setSelectedDate] = useState<string | undefined>();
  const { favorites, isFavorite, toggleFavorite } = useFavorites();
  const [sortField, setSortField] = useState<SortField>("code");
  const [sortDirection, setSortDirection] = useState<SortDirection>("asc");
  const [showFavoritesOnly, setShowFavoritesOnly] = useState(false);

  const { data: rates, isLoading, error } = useQuery<CurrencyRate[]>({
    queryKey: ["rates", selectedDate],
    queryFn: () => fetchRatesByDate(selectedDate),
  });

  const { data: publicationDates } = useQuery({
    queryKey: ["publicationDates"],
    queryFn: fetchPublicationDates,
  });

  if (isLoading) return <p>Loading...</p>;
  if (error) return <p>Error loading data</p>;

  const filteredRates = rates
    ?.filter(r =>
      `${r.code} ${r.currency}`.toLowerCase().includes(query.toLowerCase())
    )
    .filter(r =>
      !showFavoritesOnly || favorites.includes(r.code)
    ) ?? [];

  const sortedRates = [...filteredRates].sort((a, b) => {
    const dir = sortDirection === "asc" ? 1 : -1;
    if (sortField === "currency" || sortField === "code") {
      const aStr = a[sortField]
        .normalize("NFKD")
        .toLowerCase();
      const bStr = b[sortField]
        .normalize("NFKD")
        .toLowerCase();
      return aStr.localeCompare(bStr, "pl") * dir;
    }
    return (a.mid - b.mid) * dir;
  });

  return (
    <>
      <div className="display: flex">
        <label className="flex items-center gap-2 whitespace-nowrap cursor-pointer">
          <input
            type="checkbox"
            checked={showFavoritesOnly}
            onChange={(e) => setShowFavoritesOnly(e.target.checked)}
          />
          Favorites only
        </label>
        
        <input
          type="text"
          placeholder="Search (code or name)"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          className="flex rounded border px-6 py-6"
        />
        
        <select
          value={selectedDate ?? ""}
          onChange={(e) =>
            setSelectedDate(e.target.value || undefined)
          }
          className="ml-auto flex rounded border px-4 py-2 min-w-[150px]"
        >
          <option value="">Latest</option>
          {publicationDates?.map((date) => (
            <option key={date} value={date}>
              {new Date(date).toLocaleDateString("pl-PL")}
            </option>
          ))}
        </select>
      </div>

      <RatesTable
        rates={sortedRates}
        onSort={(field) => {
          if (field === sortField) {
            setSortDirection(sortDirection === "asc" ? "desc" : "asc");
          } else {
            setSortField(field);
            setSortDirection("asc");
          }
        }}
        sortField={sortField}
        sortDirection={sortDirection}
        isFavorite={isFavorite}
        toggleFavorite={toggleFavorite}
      />
    </>
  );
};