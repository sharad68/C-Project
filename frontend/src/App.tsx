import { useEffect, useState, type FormEvent, type ReactNode } from "react";
import {
  BrowserRouter,
  Link,
  NavLink,
  Navigate,
  Route,
  Routes,
  useLocation,
  useNavigate,
  useParams,
} from "react-router-dom";
import { AuthProvider, useAuth } from "./auth/AuthContext";
import {
  api,
  formatDate,
  formatKickoff,
  type AdminIdentity,
  type AdminUserUpsertRequest,
  type ChangeLogEntry,
  type Competition,
  type Match,
  type Person,
  type PersonProfile,
  type PlayerRanking,
  type SeasonDetail,
  type Sport,
  type Team,
  type TeamRoster,
} from "./lib/api";
import "./App.css";

interface DashboardData {
  sports: Sport[];
  competitions: Competition[];
  liveMatches: Match[];
  upcomingMatches: Match[];
  teams: Team[];
  people: Person[];
}

function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <AppShell />
      </BrowserRouter>
    </AuthProvider>
  );
}

function AppShell() {
  const { isAuthenticated, session, logout } = useAuth();
  const [data, setData] = useState<DashboardData>({
    sports: [],
    competitions: [],
    liveMatches: [],
    upcomingMatches: [],
    teams: [],
    people: [],
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async () => {
      try {
        const [
          sports,
          competitions,
          liveMatches,
          upcomingMatches,
          teams,
          people,
        ] = await Promise.all([
          api.getSports(),
          api.getCompetitions(),
          api.getLiveMatches(),
          api.getUpcomingMatches(),
          api.getTeams(),
          api.searchPeople(""),
        ]);

        setData({
          sports: sports.items,
          competitions: competitions.items,
          liveMatches,
          upcomingMatches,
          teams: teams.items,
          people: people.items,
        });
      } catch (loadError) {
        setError(
          loadError instanceof Error
            ? loadError.message
            : "Unable to load the sports platform right now.",
        );
      } finally {
        setLoading(false);
      }
    };

    void load();
  }, []);

  return (
    <div className="app-shell">
      <header className="topbar">
        <div className="brand-block">
          <p className="eyebrow">Sports data aggregation platform</p>
          <h1>Tornois</h1>
        </div>

        <nav className="nav-links">
          <AppNavLink to="/">Home</AppNavLink>
          <AppNavLink to="/sports">Sports</AppNavLink>
          <AppNavLink to="/competitions">Competitions</AppNavLink>
          <AppNavLink to="/teams">Teams</AppNavLink>
          <AppNavLink to="/players">Players</AppNavLink>
          <AppNavLink to="/matches">Matches</AppNavLink>
        </nav>

        <div className="auth-strip">
          {isAuthenticated ? (
            <>
              <span className="user-chip">
                {session?.user.displayName} · {session?.user.role}
              </span>
              <Link className="button-secondary" to="/admin">
                Dashboard
              </Link>
              <button className="button-ghost" onClick={logout} type="button">
                Logout
              </button>
            </>
          ) : (
            <Link className="button-secondary" to="/admin/login">
              Admin login
            </Link>
          )}
        </div>
      </header>

      <main className="layout">
        {loading ? (
          <StatePanel
            title="Loading platform data"
            message="Fetching sports, fixtures, teams, and rankings..."
          />
        ) : error ? (
          <StatePanel
            title="Backend not reachable"
            message={`${error} Start the API on http://localhost:5000 to enable live data.`}
          />
        ) : (
          <Routes>
            <Route path="/" element={<HomePage data={data} />} />
            <Route
              path="/sports"
              element={
                <SportsDirectoryPage
                  sports={data.sports}
                  competitions={data.competitions}
                />
              }
            />
            <Route
              path="/sports/:sportId"
              element={
                <SportDetailPage
                  sports={data.sports}
                  competitions={data.competitions}
                />
              }
            />
            <Route
              path="/competitions"
              element={
                <CompetitionsDirectoryPage competitions={data.competitions} />
              }
            />
            <Route
              path="/competitions/:competitionId"
              element={<CompetitionDetailPage />}
            />
            <Route
              path="/teams"
              element={<TeamsDirectoryPage teams={data.teams} />}
            />
            <Route path="/teams/:teamId" element={<TeamDetailPage />} />
            <Route
              path="/players"
              element={<PlayersDirectoryPage people={data.people} />}
            />
            <Route path="/players/:personId" element={<PlayerDetailPage />} />
            <Route
              path="/matches"
              element={
                <MatchesPage
                  liveMatches={data.liveMatches}
                  upcomingMatches={data.upcomingMatches}
                />
              }
            />
            <Route path="/matches/:matchId" element={<MatchDetailPage />} />
            <Route path="/admin/login" element={<AdminLoginPage />} />
            <Route
              path="/admin"
              element={
                <ProtectedRoute>
                  <AdminDashboardPage />
                </ProtectedRoute>
              }
            />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        )}
      </main>
    </div>
  );
}

function AppNavLink({ to, children }: { to: string; children: ReactNode }) {
  return (
    <NavLink
      className={({ isActive }) => `nav-link${isActive ? " active" : ""}`}
      to={to}
      end={to === "/"}
    >
      {children}
    </NavLink>
  );
}

function HomePage({ data }: { data: DashboardData }) {
  const stats = [
    { label: "Sports", value: data.sports.length },
    { label: "Competitions", value: data.competitions.length },
    { label: "Live matches", value: data.liveMatches.length },
    { label: "Tracked players", value: data.people.length },
  ];

  return (
    <div className="page-stack">
      <section className="hero-panel">
        <div>
          <p className="eyebrow">Unified competition hub</p>
          <h2>Browse leagues, live fixtures, rosters, and player profiles.</h2>
          <p className="muted-text">
            This MVP connects the React frontend with the ASP.NET API and covers
            public browsing plus JWT-based admin access.
          </p>
        </div>
        <div className="hero-actions">
          <Link className="button-primary" to="/matches">
            View live matches
          </Link>
          <Link className="button-secondary" to="/sports">
            Explore sports
          </Link>
        </div>
        <div className="stats-grid">
          {stats.map((stat) => (
            <article className="stat-card" key={stat.label}>
              <span>{stat.label}</span>
              <strong>{stat.value}</strong>
            </article>
          ))}
        </div>
      </section>

      <section className="panel">
        <SectionHeading
          title="Live now"
          subtitle="Currently tracked fixtures across the platform"
        />
        <div className="card-grid">
          {data.liveMatches.map((match) => (
            <MatchCard key={match.id} match={match} />
          ))}
        </div>
      </section>

      <section className="two-column">
        <div className="panel">
          <SectionHeading
            title="Sports overview"
            subtitle="Start from a sport and drill down into its competitions"
          />
          <div className="card-grid compact-grid">
            {data.sports.map((sport) => (
              <Link
                className="panel-link"
                key={sport.id}
                to={`/sports/${sport.id}`}
              >
                <h3>{sport.name}</h3>
                <p className="muted-text">{sport.description}</p>
                <span className="chip">
                  {sport.isOlympic ? "Olympic sport" : "Specialised circuit"}
                </span>
              </Link>
            ))}
          </div>
        </div>

        <div className="panel">
          <SectionHeading
            title="Upcoming fixtures"
            subtitle="Next games scheduled in the feed"
          />
          <div className="stack-list">
            {data.upcomingMatches.map((match) => (
              <Link
                className="list-row"
                key={match.id}
                to={`/matches/${match.id}`}
              >
                <div>
                  <strong>
                    {match.homeTeam} vs {match.awayTeam}
                  </strong>
                  <p className="muted-text">{match.competition}</p>
                </div>
                <span>{formatKickoff(match.kickoffUtc)}</span>
              </Link>
            ))}
          </div>
        </div>
      </section>
    </div>
  );
}

function SportsDirectoryPage({
  sports,
  competitions,
}: {
  sports: Sport[];
  competitions: Competition[];
}) {
  return (
    <section className="panel">
      <SectionHeading
        title="Sports"
        subtitle="Public catalogue of supported sports and their competitive leagues"
      />
      <div className="card-grid">
        {sports.map((sport) => {
          const sportCompetitions = competitions.filter(
            (competition) => competition.sportId === sport.id,
          );
          return (
            <article className="detail-card" key={sport.id}>
              <h3>{sport.name}</h3>
              <p className="muted-text">{sport.description}</p>
              <div className="chip-row">
                {sportCompetitions.map((competition) => (
                  <Link
                    className="chip"
                    key={competition.id}
                    to={`/competitions/${competition.id}`}
                  >
                    {competition.name}
                  </Link>
                ))}
              </div>
            </article>
          );
        })}
      </div>
    </section>
  );
}

function SportDetailPage({
  sports,
  competitions,
}: {
  sports: Sport[];
  competitions: Competition[];
}) {
  const params = useParams();
  const sport = sports.find((entry) => entry.id === Number(params.sportId));

  if (!sport) {
    return (
      <StatePanel
        title="Sport not found"
        message="Pick a sport from the directory to continue."
      />
    );
  }

  const relatedCompetitions = competitions.filter(
    (competition) => competition.sportId === sport.id,
  );

  return (
    <section className="panel">
      <SectionHeading title={sport.name} subtitle={sport.description} />
      <div className="card-grid">
        {relatedCompetitions.map((competition) => (
          <Link
            className="panel-link"
            key={competition.id}
            to={`/competitions/${competition.id}`}
          >
            <h3>{competition.name}</h3>
            <p className="muted-text">
              {competition.country} · {competition.format}
            </p>
          </Link>
        ))}
      </div>
    </section>
  );
}

function CompetitionsDirectoryPage({
  competitions,
}: {
  competitions: Competition[];
}) {
  return (
    <section className="panel">
      <SectionHeading
        title="Competitions"
        subtitle="League and cup competitions currently available for browsing"
      />
      <div className="card-grid">
        {competitions.map((competition) => (
          <Link
            className="panel-link"
            key={competition.id}
            to={`/competitions/${competition.id}`}
          >
            <h3>{competition.name}</h3>
            <p className="muted-text">{competition.country}</p>
            <span className="chip">{competition.isCup ? "Cup" : "League"}</span>
          </Link>
        ))}
      </div>
    </section>
  );
}

function CompetitionDetailPage() {
  const params = useParams();
  const competitionId = Number(params.competitionId);
  const [seasonDetail, setSeasonDetail] = useState<SeasonDetail | null>(null);
  const [rankings, setRankings] = useState<PlayerRanking[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async () => {
      try {
        const seasons = await api.getCompetitionSeasons(competitionId);
        const currentSeason = seasons.items[0];
        if (!currentSeason) {
          throw new Error("No season data available for this competition.");
        }

        const [detail, playerRankings] = await Promise.all([
          api.getSeasonDetail(currentSeason.id),
          api.getPlayerRankings(competitionId),
        ]);

        setSeasonDetail(detail);
        setRankings(playerRankings);
      } catch (loadError) {
        setError(
          loadError instanceof Error
            ? loadError.message
            : "Unable to load competition details.",
        );
      } finally {
        setLoading(false);
      }
    };

    void load();
  }, [competitionId]);

  if (loading) {
    return (
      <StatePanel
        title="Loading competition"
        message="Fetching season and ranking information..."
      />
    );
  }

  if (error || !seasonDetail) {
    return (
      <StatePanel
        title="Competition unavailable"
        message={error ?? "The requested competition could not be found."}
      />
    );
  }

  return (
    <div className="page-stack">
      <section className="panel">
        <SectionHeading
          title={`${seasonDetail.competition.name} · ${seasonDetail.season.name}`}
          subtitle={`${seasonDetail.competition.country} · ${seasonDetail.competition.format}`}
        />
        <table className="data-table">
          <thead>
            <tr>
              <th>#</th>
              <th>Team</th>
              <th>P</th>
              <th>W</th>
              <th>D</th>
              <th>L</th>
              <th>Pts</th>
            </tr>
          </thead>
          <tbody>
            {seasonDetail.standings.map((row) => (
              <tr key={row.position}>
                <td>{row.position}</td>
                <td>{row.team}</td>
                <td>{row.played}</td>
                <td>{row.won}</td>
                <td>{row.drawn}</td>
                <td>{row.lost}</td>
                <td>{row.points}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>

      <section className="panel">
        <SectionHeading
          title="Player rankings"
          subtitle="Current leaders from this competition"
        />
        <div className="stack-list">
          {rankings.map((entry) => (
            <div className="list-row" key={`${entry.person}-${entry.category}`}>
              <div>
                <strong>
                  #{entry.position} {entry.person}
                </strong>
                <p className="muted-text">
                  {entry.team} · {entry.category}
                </p>
              </div>
              <span className="chip">{entry.value}</span>
            </div>
          ))}
        </div>
      </section>
    </div>
  );
}

function TeamsDirectoryPage({ teams }: { teams: Team[] }) {
  return (
    <section className="panel">
      <SectionHeading
        title="Teams"
        subtitle="Browse clubs and squads with roster detail pages"
      />
      <div className="card-grid">
        {teams.map((team) => (
          <Link className="panel-link" key={team.id} to={`/teams/${team.id}`}>
            <h3>{team.name}</h3>
            <p className="muted-text">
              {team.country} · {team.venue}
            </p>
            <span className="chip">Founded {team.founded}</span>
          </Link>
        ))}
      </div>
    </section>
  );
}

function TeamDetailPage() {
  const params = useParams();
  const [roster, setRoster] = useState<TeamRoster | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async () => {
      try {
        setRoster(await api.getTeamRoster(Number(params.teamId)));
      } catch (loadError) {
        setError(
          loadError instanceof Error
            ? loadError.message
            : "Unable to load roster.",
        );
      } finally {
        setLoading(false);
      }
    };

    void load();
  }, [params.teamId]);

  if (loading) {
    return (
      <StatePanel
        title="Loading roster"
        message="Fetching team members and details..."
      />
    );
  }

  if (error || !roster) {
    return (
      <StatePanel
        title="Team unavailable"
        message={error ?? "The selected team could not be found."}
      />
    );
  }

  return (
    <section className="panel">
      <SectionHeading
        title={roster.team.name}
        subtitle={`${roster.team.country} · ${roster.team.venue}`}
      />
      <div className="card-grid">
        {roster.members.map((member) => (
          <Link
            className="panel-link"
            key={member.id}
            to={`/players/${member.id}`}
          >
            <h3>{member.fullName}</h3>
            <p className="muted-text">
              {member.role} · {member.nationality}
            </p>
            <span className="chip">#{member.shirtNumber ?? "—"}</span>
          </Link>
        ))}
      </div>
    </section>
  );
}

function PlayersDirectoryPage({ people }: { people: Person[] }) {
  return (
    <section className="panel">
      <SectionHeading
        title="Players & staff"
        subtitle="Search-ready directory of tracked people profiles"
      />
      <div className="card-grid">
        {people.map((person) => (
          <Link
            className="panel-link"
            key={person.id}
            to={`/players/${person.id}`}
          >
            <h3>{person.fullName}</h3>
            <p className="muted-text">
              {person.role} · {person.nationality}
            </p>
            <span className="chip">Born {formatDate(person.birthDate)}</span>
          </Link>
        ))}
      </div>
    </section>
  );
}

function PlayerDetailPage() {
  const params = useParams();
  const [profile, setProfile] = useState<PersonProfile | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async () => {
      try {
        setProfile(await api.getPerson(Number(params.personId)));
      } catch (loadError) {
        setError(
          loadError instanceof Error
            ? loadError.message
            : "Unable to load profile.",
        );
      } finally {
        setLoading(false);
      }
    };

    void load();
  }, [params.personId]);

  if (loading) {
    return (
      <StatePanel
        title="Loading player profile"
        message="Fetching biography and team details..."
      />
    );
  }

  if (error || !profile) {
    return (
      <StatePanel
        title="Player unavailable"
        message={error ?? "The selected person could not be found."}
      />
    );
  }

  return (
    <section className="panel">
      <SectionHeading
        title={profile.person.fullName}
        subtitle={`${profile.person.role} · ${profile.teamName}`}
      />
      <div className="info-grid">
        <div>
          <p className="muted-text">Nationality</p>
          <strong>{profile.person.nationality}</strong>
        </div>
        <div>
          <p className="muted-text">Birth date</p>
          <strong>{formatDate(profile.person.birthDate)}</strong>
        </div>
        <div>
          <p className="muted-text">Shirt number</p>
          <strong>{profile.person.shirtNumber ?? "—"}</strong>
        </div>
      </div>
      <p className="bio-copy">{profile.bio}</p>
    </section>
  );
}

function MatchesPage({
  liveMatches,
  upcomingMatches,
}: {
  liveMatches: Match[];
  upcomingMatches: Match[];
}) {
  return (
    <div className="page-stack">
      <section className="panel">
        <SectionHeading
          title="Live matches"
          subtitle="Real-time scoreboard feed from the platform"
        />
        <div className="card-grid">
          {liveMatches.map((match) => (
            <MatchCard key={match.id} match={match} />
          ))}
        </div>
      </section>

      <section className="panel">
        <SectionHeading
          title="Upcoming fixtures"
          subtitle="Scheduled games and kickoff information"
        />
        <div className="stack-list">
          {upcomingMatches.map((match) => (
            <Link
              className="list-row"
              key={match.id}
              to={`/matches/${match.id}`}
            >
              <div>
                <strong>
                  {match.homeTeam} vs {match.awayTeam}
                </strong>
                <p className="muted-text">{match.venue}</p>
              </div>
              <span>{formatKickoff(match.kickoffUtc)}</span>
            </Link>
          ))}
        </div>
      </section>
    </div>
  );
}

function MatchDetailPage() {
  const params = useParams();
  const [detail, setDetail] = useState<Awaited<
    ReturnType<typeof api.getMatchDetail>
  > | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async () => {
      try {
        setDetail(await api.getMatchDetail(Number(params.matchId)));
      } catch (loadError) {
        setError(
          loadError instanceof Error
            ? loadError.message
            : "Unable to load match detail.",
        );
      } finally {
        setLoading(false);
      }
    };

    void load();
  }, [params.matchId]);

  if (loading) {
    return (
      <StatePanel
        title="Loading match detail"
        message="Fetching fixture timeline and score data..."
      />
    );
  }

  if (error || !detail) {
    return (
      <StatePanel
        title="Match unavailable"
        message={error ?? "The selected fixture could not be found."}
      />
    );
  }

  const { match, events } = detail;

  return (
    <section className="panel">
      <SectionHeading
        title={`${match.homeTeam} ${match.homeScore} - ${match.awayScore} ${match.awayTeam}`}
        subtitle={`${match.competition} · ${match.status} · ${formatKickoff(match.kickoffUtc)}`}
      />
      <div className="stack-list">
        {events.map((event, index) => (
          <div className="list-row" key={`${event.subject}-${index}`}>
            <div>
              <strong>{event.subject}</strong>
              <p className="muted-text">{event.type}</p>
            </div>
            <span className="chip">{event.minute}'</span>
          </div>
        ))}
      </div>
    </section>
  );
}

function AdminLoginPage() {
  const { isAuthenticated, login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [userName, setUserName] = useState("superadmin");
  const [password, setPassword] = useState("Pass@123");
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  if (isAuthenticated) {
    return <Navigate to="/admin" replace />;
  }

  const onSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setSubmitting(true);
    setError(null);

    try {
      await login(userName, password);
      const target = ((location.state as { from?: string } | null)?.from ??
        "/admin") as string;
      navigate(target, { replace: true });
    } catch (loginError) {
      setError(
        loginError instanceof Error ? loginError.message : "Login failed.",
      );
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <section className="panel narrow-panel">
      <SectionHeading
        title="Admin login"
        subtitle="JWT authentication for protected management views"
      />
      <form className="form-stack" onSubmit={onSubmit}>
        <label>
          <span>Username</span>
          <input
            className="input"
            onChange={(event) => setUserName(event.target.value)}
            value={userName}
          />
        </label>
        <label>
          <span>Password</span>
          <input
            className="input"
            onChange={(event) => setPassword(event.target.value)}
            type="password"
            value={password}
          />
        </label>
        {error ? <p className="error-text">{error}</p> : null}
        <button className="button-primary" disabled={submitting} type="submit">
          {submitting ? "Signing in..." : "Sign in"}
        </button>
      </form>
      <p className="muted-text">
        Demo seed users: `superadmin / Pass@123`, `editor / Editor@123`.
      </p>
    </section>
  );
}

function AdminDashboardPage() {
  const { session } = useAuth();
  const [changes, setChanges] = useState<ChangeLogEntry[]>([]);
  const [admins, setAdmins] = useState<AdminIdentity[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [formState, setFormState] = useState<AdminUserUpsertRequest>({
    userName: "",
    displayName: "",
    role: "readonly",
    password: "",
    isActive: true,
  });

  const isSuperadmin = session?.user.role === "superadmin";

  useEffect(() => {
    const load = async () => {
      if (!session) {
        return;
      }

      try {
        const [auditEntries, adminUsers] = await Promise.all([
          api.getAdminChanges(session.token),
          isSuperadmin ? api.getAdminUsers(session.token) : Promise.resolve([]),
        ]);

        setChanges(auditEntries);
        setAdmins(adminUsers);
      } catch (loadError) {
        setError(
          loadError instanceof Error
            ? loadError.message
            : "Unable to load admin activity.",
        );
      } finally {
        setLoading(false);
      }
    };

    void load();
  }, [isSuperadmin, session]);

  if (!session) {
    return null;
  }

  const handleCreateAdmin = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);
    setSuccessMessage(null);

    try {
      const createdUser = await api.upsertAdminUser(session.token, formState);
      setAdmins((current) => {
        const others = current.filter(
          (entry) => entry.userName !== createdUser.userName,
        );
        return [...others, createdUser].sort((left, right) =>
          left.userName.localeCompare(right.userName),
        );
      });
      setFormState({
        userName: "",
        displayName: "",
        role: "readonly",
        password: "",
        isActive: true,
      });
      setSuccessMessage(`Saved admin user ${createdUser.userName}.`);
    } catch (saveError) {
      setError(
        saveError instanceof Error
          ? saveError.message
          : "Unable to save admin user.",
      );
    }
  };

  return (
    <div className="page-stack">
      <section className="panel">
        <SectionHeading
          title="Admin dashboard"
          subtitle="Protected operational overview for authenticated users"
        />
        <div className="info-grid">
          <div>
            <p className="muted-text">Signed in as</p>
            <strong>{session.user.displayName}</strong>
          </div>
          <div>
            <p className="muted-text">Role</p>
            <strong>{session.user.role}</strong>
          </div>
          <div>
            <p className="muted-text">Token expires</p>
            <strong>{formatKickoff(session.expiresAtUtc)}</strong>
          </div>
        </div>
      </section>

      {isSuperadmin ? (
        <section className="panel">
          <SectionHeading
            title="Admin user management"
            subtitle="Create or update superadmin, editor, and readonly accounts"
          />
          <div className="two-column">
            <form className="form-stack" onSubmit={handleCreateAdmin}>
              <label>
                <span>Username</span>
                <input
                  className="input"
                  value={formState.userName}
                  onChange={(event) =>
                    setFormState((current) => ({
                      ...current,
                      userName: event.target.value,
                    }))
                  }
                />
              </label>
              <label>
                <span>Display name</span>
                <input
                  className="input"
                  value={formState.displayName}
                  onChange={(event) =>
                    setFormState((current) => ({
                      ...current,
                      displayName: event.target.value,
                    }))
                  }
                />
              </label>
              <label>
                <span>Role</span>
                <select
                  className="input"
                  value={formState.role}
                  onChange={(event) =>
                    setFormState((current) => ({
                      ...current,
                      role: event.target
                        .value as AdminUserUpsertRequest["role"],
                    }))
                  }
                >
                  <option value="superadmin">superadmin</option>
                  <option value="editor">editor</option>
                  <option value="readonly">readonly</option>
                </select>
              </label>
              <label>
                <span>Password</span>
                <input
                  className="input"
                  type="password"
                  value={formState.password}
                  onChange={(event) =>
                    setFormState((current) => ({
                      ...current,
                      password: event.target.value,
                    }))
                  }
                />
              </label>
              <button className="button-primary" type="submit">
                Save admin user
              </button>
              {successMessage ? (
                <p className="muted-text">{successMessage}</p>
              ) : null}
            </form>

            <div className="stack-list">
              {admins.map((admin) => (
                <div className="list-row" key={admin.userName}>
                  <div>
                    <strong>{admin.displayName}</strong>
                    <p className="muted-text">{admin.userName}</p>
                  </div>
                  <span className="chip">{admin.role}</span>
                </div>
              ))}
            </div>
          </div>
        </section>
      ) : null}

      <section className="panel">
        <SectionHeading
          title="Recent sync and audit activity"
          subtitle="Seeded change log entries from the backend"
        />
        {loading ? (
          <p className="muted-text">Loading audit entries...</p>
        ) : error ? (
          <p className="error-text">{error}</p>
        ) : (
          <div className="stack-list">
            {changes.map((entry) => (
              <div className="list-row" key={entry.id}>
                <div>
                  <strong>
                    {entry.action} · {entry.entityName}
                  </strong>
                  <p className="muted-text">{entry.summary}</p>
                </div>
                <span>{formatKickoff(entry.timestamp)}</span>
              </div>
            ))}
          </div>
        )}
      </section>
    </div>
  );
}

function ProtectedRoute({ children }: { children: ReactNode }) {
  const { isAuthenticated, isLoading } = useAuth();
  const location = useLocation();

  if (isLoading) {
    return (
      <StatePanel
        title="Checking session"
        message="Verifying your admin token..."
      />
    );
  }

  if (!isAuthenticated) {
    return (
      <Navigate to="/admin/login" replace state={{ from: location.pathname }} />
    );
  }

  return <>{children}</>;
}

function MatchCard({ match }: { match: Match }) {
  return (
    <Link className="panel-link" to={`/matches/${match.id}`}>
      <div className="match-header">
        <span className="chip">{match.status}</span>
        <span className="muted-text">{match.competition}</span>
      </div>
      <h3>
        {match.homeTeam} vs {match.awayTeam}
      </h3>
      <p className="score-line">
        {match.homeScore} - {match.awayScore}
      </p>
      <p className="muted-text">{formatKickoff(match.kickoffUtc)}</p>
    </Link>
  );
}

function SectionHeading({
  title,
  subtitle,
}: {
  title: string;
  subtitle: string;
}) {
  return (
    <div className="section-heading">
      <h2>{title}</h2>
      <p className="muted-text">{subtitle}</p>
    </div>
  );
}

function StatePanel({ title, message }: { title: string; message: string }) {
  return (
    <section className="panel state-panel">
      <h2>{title}</h2>
      <p className="muted-text">{message}</p>
    </section>
  );
}

export default App;
