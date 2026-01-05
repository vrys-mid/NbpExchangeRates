import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { fetchLatestRates } from "../api/ratesApi";
import { RatesTable } from "../components/RatesTable";

type SortField = "code" | "currency" | "mid";
type SortDirection = "asc" | "desc";


export const HomePage = () => {
  const [query, setQuery] = useState("");

const [sortField, setSortField] = useState<SortField>("code");
const [sortDirection, setSortDirection] = useState<SortDirection>("asc");




  const { data, isLoading, error } = useQuery({
    queryKey: ["rates", "latest"],
    queryFn: fetchLatestRates,
  });

  if (isLoading) return <p>Loading...</p>;
  if (error) return <p>Error loading data</p>;

  const filteredRates = data!.filter((r) =>
    `${r.code} ${r.currency}`
      .toLowerCase()
      .includes(query.toLowerCase())
  );
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
      <input
        type="text"
        placeholder="Search currency (code or name)..."
        value={query}
        onChange={(e) => setQuery(e.target.value)}
        className="mb-4 w-full rounded border px-4 py-2"
      />

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
/>
    </>
  );
};
