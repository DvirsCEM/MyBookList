import { getSearchParam, send } from "clientUtilities";
import { get } from "componentUtilities";
import { createBar, createPreview } from "scripts/funcs";
import { Author, Book, User } from "scripts/types";

var authorBooksH2 = get("h2", "authorBooksH2");
var authorBooksDiv = get("div", "authorBooksDiv");
var token = localStorage.getItem("token");

var user = await send<User | null>("getUser", token);

document.body.prepend(createBar(user));

var authorId = Number(getSearchParam("authorId"));
var author = await send<Author>("getAuthor", authorId);
var books = await send<Book[]>("getBooksByAuthor", authorId);

authorBooksH2.innerText = `Books by ${author.name}`;

for (var book of books) {
  authorBooksDiv.append(createPreview(book));
}
