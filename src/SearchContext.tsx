import { createContext, useContext, useState, type ReactNode } from 'react';
import type { SearchParams } from './types';

const LS_KEY = 'search_params';

function today()    { return new Date().toISOString().slice(0, 10); }
function tomorrow() {
  const d = new Date();
  d.setDate(d.getDate() + 1);
  return d.toISOString().slice(0, 10);
}

const DEFAULT_PARAMS: SearchParams = {
  checkIn:  today(),
  checkOut: tomorrow(),
  adults:   2,
  children: 0,
};

function readFromStorage(): SearchParams {
  try {
    const raw = localStorage.getItem(LS_KEY);
    if (raw) return { ...DEFAULT_PARAMS, ...JSON.parse(raw) };
  } catch { /* ignore */ }
  return DEFAULT_PARAMS;
}

interface SearchContextValue {
  params: SearchParams;
  setParams: (p: SearchParams) => void;
}

const SearchContext = createContext<SearchContextValue>({
  params: DEFAULT_PARAMS,
  setParams: () => {},
});

export function SearchProvider({ children }: { children: ReactNode }) {
  const [params, setParamsState] = useState<SearchParams>(readFromStorage);

  function setParams(p: SearchParams) {
    setParamsState(p);
    localStorage.setItem(LS_KEY, JSON.stringify(p));
  }

  return (
    <SearchContext.Provider value={{ params, setParams }}>
      {children}
    </SearchContext.Provider>
  );
}

export function useSearch() {
  return useContext(SearchContext);
}
