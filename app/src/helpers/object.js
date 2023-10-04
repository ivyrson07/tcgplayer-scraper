export function updateObjectProperty(object, propertyName, newValue) {
    const updatedObject = { ...object };
    
    updatedObject[propertyName] = newValue;

    return updatedObject;
}