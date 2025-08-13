import { create, style } from "../componentUtilities";
import { Book } from "../tools/types";
import { createPreview } from "./preview";

export var createShelf = (title: string, books: Book[]) => {
  var previewsDiv = books.map(book => createPreview(book));

  return style("/website/components/shelf.css",
    create("h2", { id: "titleH2" }, [title]),
    create("div", { id: "previewsDiv" }, previewsDiv)
  );

};
