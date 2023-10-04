import { useDataContext } from '../context/DataContext';

function Counter() {
    const {data, setData} = useDataContext();

    const handleSetCountAdd = () => {
        setData({...data, counter: data.counter + 1});
    }
    const handleSetCountSubtract = () => { 
        setData({...data, counter: data.counter - 1});
    }

    return ( 
        <div>
            <button type="button" onClick={handleSetCountAdd}>+</button>
            <button type="button" onClick={handleSetCountSubtract}>-</button>
        </div>
    );
}

export default Counter;