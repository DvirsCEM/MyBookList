import { create, style } from "../componentUtilities";
import { Book } from "../tools/types"

export var createPreview = (book: Book) => {
  return style(
    "/website/components/preview.css",
    create("a", { href: `/website/pages/book.html?bookId=${book.id}` }, [
      create("img", { id: "coverImg", src: book.image }),
      create("div", { id: "titleDiv" }, [book.title])
    ]));
}