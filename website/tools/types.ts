type User = {
  id: string,
  name: string,
  // I have a problem that whenever I send a Book, it contains it's uploader,
  // which contains the uploader's password!!!
  // So do I just give up on the types?
  // Do I edit the Respond method to exclude passwords?
}

type Author = {
  id: number,
  name: string,
}

type Book = {
  id: number,
  title: string,
  image: string,
  description: string,
  author: Author,
  uploader: User,
};