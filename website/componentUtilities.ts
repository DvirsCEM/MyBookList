export function create<
  K extends keyof HTMLElementTagNameMap,
>(
  tagName: K,
  props: Partial<HTMLElementTagNameMap[K]> | null = null,
  children: (HTMLElement | string)[] | null = null,
): HTMLElementTagNameMap[K] {
  const el = document.createElement(tagName) as HTMLElementTagNameMap[K];

  if (props) {
    Object.assign(el, props);
  }

  if (children) {
    for (const child of children) {
      el.append(child);
    }
  }

  return el;
}

export function style(cssPath: string, element: HTMLElement): HTMLElement {
  const wrapper = document.createElement("div");
  wrapper.style.display = "none";
  const shadow = wrapper.attachShadow({ mode: "open" });

  fetch(cssPath)
    .then((response) => response.text())
    .then((cssText) => {
      shadow.prepend(create("style", { textContent: cssText }));
      wrapper.style.display = "block";
    });
  shadow.appendChild(element);
  return wrapper;
}
