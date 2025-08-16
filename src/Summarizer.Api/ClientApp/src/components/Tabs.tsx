import React, { useState } from "react";

type TabProps = { label: string; children: React.ReactNode };
type TabChild = React.ReactElement<TabProps>;

export function Tabs({ children }: { children: TabChild[] | TabChild }) {
  const tabs = React.Children.toArray(children) as TabChild[];
  const [active, setActive] = useState(0);

  return (
    <div>
      <div className="flex gap-3 border-b mb-4">
        {tabs.map((t, i) => (
          <button
            key={i}
            onClick={() => setActive(i)}
            className={`px-3 py-2 ${
              i === active ? "border-b-2 font-semibold" : "opacity-70"
            }`}
          >
            {t.props.label}
          </button>
        ))}
      </div>
      <div>{tabs[active]}</div>
    </div>
  );
}

export function Tab(_props: TabProps) {
  return <>{_props.children}</>;
}
