import { send } from "clientUtilities";
import { get } from "componentUtilities";
import { createBar, createPreview } from "scripts/funcs";
import { Book, User } from "scripts/types";

var allBooksDiv = get("div", "allBooksDiv");
var favoriteBooksH2 = get("h2", "favoriteBooksH2");
var favoriteBooksDiv = get("div", "favoriteBooksDiv");

var token = localStorage.getItem("token");
var user = await send<User | null>("getUser", token);

document.body.prepend(createBar(user));

var books = await send<Book[]>("getAllBooks");
for (var book of books) {
  allBooksDiv.append(createPreview(book));
}

if (user != null) {
  var favoriteBooks = await send<Book[]>("getFavoriteBooks", token);

  if (favoriteBooks.length > 0) {
    favoriteBooksH2.style.visibility = "visible";
    for (var book of favoriteBooks) {
      favoriteBooksDiv.append(createPreview(book));
    }
  }
}
