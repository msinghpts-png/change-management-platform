import "./CalendarPage.css";

const dayLabels = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
const days = Array.from({ length: 31 }, (_, i) => i + 1);

const CalendarPage = () => {
  return (
    <section>
      <div className="page-header">
        <div>
          <h2 className="page-title">Change Calendar</h2>
          <p className="page-subtitle">View planned windows and operational changes</p>
        </div>
      </div>

      <div className="card">
        <div className="calendar-header-row">
          <h3 className="calendar-month">January 2026</h3>
          <button type="button" className="btn btn-secondary">
            Today
          </button>
        </div>
        <div className="calendar-grid">
          {dayLabels.map((label) => (
            <div key={label} className="calendar-label">
              {label}
            </div>
          ))}
          {days.map((day) => (
            <div key={day} className={`calendar-cell ${day === 28 ? "active" : ""}`}>
              <span className="calendar-day">{day}</span>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
};

export default CalendarPage;
