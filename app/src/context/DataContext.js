import { createContext, useContext, useState } from 'react';

const DataContext = createContext();

export function DataContextProvider({ children }) {
    const object = {
        counter: 0
    };

    const [data, setData] = useState(object);

    return (
        <DataContext.Provider value={{ data, setData }}>
            {children}
        </DataContext.Provider>
    );
}

export function useDataContext() {
    return useContext(DataContext);
}

export function updateObjectProperty(propertyName, value) {
    
}