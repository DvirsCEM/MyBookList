import { create } from "componentUtilities";
import { Book, User } from "scripts/types";

export function createBar(user: User | null) {
  var barRightDiv: HTMLDivElement;
  if (user == null) {
    barRightDiv = create("div", { className: "barRightDiv" },
      create("a", { innerText: "Sign Up", href: "signUp.html" }),
      create("a", { innerText: "Log In", href: "logIn.html" }),
    );
  }
  else {
    barRightDiv = create("div", { className: "barRightDiv" },
      create("div", { innerText: `Welcome, ${user.username}!` }),
      create("button", { innerText: "Log Out", onclick: onLogOutClick })
    )
  }

  return create("div", { className: "barDiv" },
    create("a", { className: "barLeftA", href: "index.html" }, "MyBookList"),
    barRightDiv,
  );
}

function onLogOutClick() {
  localStorage.removeItem("token");
  location.reload();
}

export function createPreview(book: Book) {
  return create("a", { className: "previewA", href: `book.html?bookId=${book.id}` },
    create("img", { className: "previewImg", src: book.imageUrl }),
    book.title
  )
}