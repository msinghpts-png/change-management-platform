import React from "react";

type State = { hasError: boolean; message?: string };

export class ErrorBoundary extends React.Component<React.PropsWithChildren, State> {
  public state: State = { hasError: false };

  public static getDerivedStateFromError(error: Error): State {
    return { hasError: true, message: error.message };
  }

  public componentDidCatch(error: Error) {
    console.error("Unhandled UI error", error);
  }

  public render() {
    if (this.state.hasError) {
      return <div className="card card-pad">Something went wrong: {this.state.message}</div>;
    }

    return this.props.children;
  }
}
