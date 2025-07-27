import { create } from "../componentUtilities";

var createBanner = () => {
  var banner = create("div", {}, [
    create("a", { href: "index.html" }, [
      create("h1", {}, ["My Book List"]),
    ]),
    create("div", { id: "userDiv" }),
  ]);

  
};
