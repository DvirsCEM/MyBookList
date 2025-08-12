export type User = {
  username: string,
}

export type Author = {
  id: number,
  name: string,
}

export type Book = {
  id: number,
  title: string,
  image: string,
  description: string,
  author: Author,
  uploader: User,
};