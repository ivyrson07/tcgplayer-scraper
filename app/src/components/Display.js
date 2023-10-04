import { useDataContext } from '../context/DataContext';

function Display(props) {
    const {data} = useDataContext();

    return (
        <>
            Count me : {data.counter}
        </>
    );
}

export default Display;