export const getInitials = (name: string) => {
  return name.match(/(\b\S)?/g)?.join('')?.match(/(^\S|\S$)?/g)?.join('')?.toUpperCase() || '';
};
// credits: Chickens https://stackoverflow.com/questions/33076177/getting-name-initials-using-js